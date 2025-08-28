using System.Linq.Expressions;
using System.Reflection;
using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// Object validation schema
/// </summary>
public class SodObject<T> : SodSchema<T> where T : new()
{
    private readonly Dictionary<string, FieldSchema> _schemaFields = new();
    private bool _strict = false;
    private bool _passthrough = false;
    private bool _strip = true;
    
    private class FieldSchema
    {
        public SodSchema<object> Schema { get; set; } = null!;
        public PropertyInfo Property { get; set; } = null!;
        public bool IsRequired { get; set; } = true;
    }

    public SodObject<T> Field<TField>(Expression<Func<T, TField>> fieldSelector, SodSchema<TField> schema)
    {
        if (fieldSelector.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Field selector must select a valid property.");

        var property = memberExpression.Member as PropertyInfo 
            ?? throw new ArgumentException("Field selector must select a property.");

        var propertyName = property.Name;
        _schemaFields[propertyName] = new FieldSchema
        {
            Schema = new SodWrapperSchema<TField>(schema),
            Property = property,
            IsRequired = !schema._isOptional
        };

        return this;
    }

    public SodObject<T> Shape(Action<SodObject<T>> shapeBuilder)
    {
        shapeBuilder(this);
        return this;
    }

    /// <summary>
    /// Makes the object validation strict (no unknown keys allowed)
    /// </summary>
    public SodObject<T> Strict()
    {
        _strict = true;
        _passthrough = false;
        _strip = false;
        return this;
    }

    /// <summary>
    /// Passes through unknown keys
    /// </summary>
    public SodObject<T> Passthrough()
    {
        _passthrough = true;
        _strict = false;
        _strip = false;
        return this;
    }

    /// <summary>
    /// Strips unknown keys (default behavior)
    /// </summary>
    public SodObject<T> Strip()
    {
        _strip = true;
        _strict = false;
        _passthrough = false;
        return this;
    }

    /// <summary>
    /// Extends this schema with fields from another schema
    /// </summary>
    public SodObject<T> Extend<TOther>(SodObject<TOther> other) where TOther : new()
    {
        // This would require more complex implementation to merge schemas
        throw new NotImplementedException("Extend is not yet implemented");
    }

    /// <summary>
    /// Makes certain fields optional
    /// </summary>
    public SodObject<T> Partial()
    {
        foreach (var field in _schemaFields.Values)
        {
            field.IsRequired = false;
        }
        return this;
    }

    /// <summary>
    /// Makes all optional fields required
    /// </summary>
    public SodObject<T> Required()
    {
        foreach (var field in _schemaFields.Values)
        {
            field.IsRequired = true;
        }
        return this;
    }

    /// <summary>
    /// Picks only specified keys from the schema
    /// </summary>
    public SodObject<T> Pick(params string[] keys)
    {
        var keysToRemove = _schemaFields.Keys.Except(keys).ToList();
        foreach (var key in keysToRemove)
        {
            _schemaFields.Remove(key);
        }
        return this;
    }

    /// <summary>
    /// Omits specified keys from the schema
    /// </summary>
    public SodObject<T> Omit(params string[] keys)
    {
        foreach (var key in keys)
        {
            _schemaFields.Remove(key);
        }
        return this;
    }

    public override SodResult<T> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue!);
            if (_isOptional || _isNullable)
                return Ok(default!);
            return Fail("Expected an object, got null");
        }

        IDictionary<string, object?> dict;
        
        if (input is IDictionary<string, object?> d)
        {
            dict = d;
        }
        else if (input is T existingObj)
        {
            // If input is already of type T, validate it
            dict = new Dictionary<string, object?>();
            foreach (var prop in typeof(T).GetProperties())
            {
                dict[prop.Name] = prop.GetValue(existingObj);
            }
        }
        else
        {
            // Try to convert object properties to dictionary
            dict = new Dictionary<string, object?>();
            var type = input.GetType();
            foreach (var prop in type.GetProperties())
            {
                dict[prop.Name] = prop.GetValue(input);
            }
        }

        var result = new T();
        var errors = new List<string>();

        // Check for unknown keys if strict
        if (_strict)
        {
            var unknownKeys = dict.Keys.Except(_schemaFields.Keys).ToList();
            if (unknownKeys.Any())
            {
                errors.Add($"Unexpected keys: {string.Join(", ", unknownKeys)}");
            }
        }

        // Validate defined fields
        foreach (var field in _schemaFields)
        {
            if (!dict.TryGetValue(field.Key, out var value) || value == null)
            {
                if (field.Value.IsRequired && !field.Value.Schema._hasDefault)
                {
                    errors.Add($"Missing required field '{field.Key}'");
                    continue;
                }
                else if (field.Value.Schema._hasDefault)
                {
                    // Apply default value
                    field.Value.Property.SetValue(result, field.Value.Schema._defaultValue);
                    continue;
                }
                else
                {
                    // Optional field, skip
                    continue;
                }
            }

            var parsed = field.Value.Schema.Parse(value);
            if (!parsed.Success)
            {
                errors.Add($"Field '{field.Key}': {parsed.Error}");
            }
            else
            {
                field.Value.Property.SetValue(result, parsed.Data);
            }
        }

        // Handle passthrough of unknown fields (if T has additional properties)
        if (_passthrough)
        {
            // This would require dynamic object handling or a base class with extra properties
            // For now, we'll just ignore extra fields
        }

        if (errors.Any())
            return Fail(errors);

        return ApplyRefinements(result);
    }

    private class SodWrapperSchema<TField> : SodSchema<object>
    {
        private readonly SodSchema<TField> _innerSchema;

        public SodWrapperSchema(SodSchema<TField> schema)
        {
            _innerSchema = schema;
            _isOptional = schema._isOptional;
            _isNullable = schema._isNullable;
            _hasDefault = schema._hasDefault;
            _defaultValue = schema._defaultValue;
        }

        public override SodResult<object> Parse(object? input)
        {
            var result = _innerSchema.Parse(input);
            return result.Success
                ? Ok((object?)result.Data)
                : Fail(result.Errors);
        }
    }
}

/// <summary>
/// Record validation schema for dictionary/map-like objects
/// </summary>
public class SodRecord<TValue> : SodSchema<Dictionary<string, TValue>>
{
    private readonly SodSchema<TValue> _valueSchema;
    private readonly SodSchema<string>? _keySchema;

    public SodRecord(SodSchema<TValue> valueSchema, SodSchema<string>? keySchema = null)
    {
        _valueSchema = valueSchema;
        _keySchema = keySchema;
    }

    public override SodResult<Dictionary<string, TValue>> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue!);
            if (_isOptional || _isNullable)
                return Ok(null!);
            return Fail("Expected a record/dictionary, got null");
        }

        if (input is not IDictionary<string, object> dict)
        {
            // Try to convert to dictionary
            if (input is System.Collections.IDictionary d)
            {
                dict = new Dictionary<string, object>();
                foreach (var key in d.Keys)
                {
                    dict[key.ToString()!] = d[key]!;
                }
            }
            else
            {
                return Fail($"Expected a dictionary, got {input.GetType().Name}");
            }
        }

        var result = new Dictionary<string, TValue>();
        var errors = new List<string>();

        foreach (var kvp in dict)
        {
            // Validate key if key schema is provided
            if (_keySchema != null)
            {
                var keyResult = _keySchema.Parse(kvp.Key);
                if (!keyResult.Success)
                {
                    errors.Add($"Key '{kvp.Key}': {keyResult.Error}");
                    continue;
                }
            }

            // Validate value
            var valueResult = _valueSchema.Parse(kvp.Value);
            if (!valueResult.Success)
            {
                errors.Add($"Value for key '{kvp.Key}': {valueResult.Error}");
            }
            else
            {
                result[kvp.Key] = valueResult.Data!;
            }
        }

        if (errors.Any())
            return Fail(errors);

        return ApplyRefinements(result);
    }
}

/// <summary>
/// Map validation schema
/// </summary>
public class SodMap<TKey, TValue> : SodSchema<Dictionary<TKey, TValue>> where TKey : notnull
{
    private readonly SodSchema<TKey> _keySchema;
    private readonly SodSchema<TValue> _valueSchema;

    public SodMap(SodSchema<TKey> keySchema, SodSchema<TValue> valueSchema)
    {
        _keySchema = keySchema;
        _valueSchema = valueSchema;
    }

    public override SodResult<Dictionary<TKey, TValue>> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue!);
            if (_isOptional || _isNullable)
                return Ok(null!);
            return Fail("Expected a map, got null");
        }

        // Handle various map-like inputs
        var result = new Dictionary<TKey, TValue>();
        var errors = new List<string>();

        if (input is IEnumerable<KeyValuePair<object, object>> pairs)
        {
            foreach (var pair in pairs)
            {
                var keyResult = _keySchema.Parse(pair.Key);
                var valueResult = _valueSchema.Parse(pair.Value);

                if (!keyResult.Success)
                    errors.Add($"Key validation failed: {keyResult.Error}");
                else if (!valueResult.Success)
                    errors.Add($"Value validation failed for key: {valueResult.Error}");
                else
                    result[keyResult.Data!] = valueResult.Data!;
            }
        }
        else if (input is System.Collections.IDictionary dict)
        {
            foreach (var key in dict.Keys)
            {
                var keyResult = _keySchema.Parse(key);
                var valueResult = _valueSchema.Parse(dict[key]);

                if (!keyResult.Success)
                    errors.Add($"Key validation failed: {keyResult.Error}");
                else if (!valueResult.Success)
                    errors.Add($"Value validation failed: {valueResult.Error}");
                else
                    result[keyResult.Data!] = valueResult.Data!;
            }
        }
        else
        {
            return Fail($"Expected a map, got {input.GetType().Name}");
        }

        if (errors.Any())
            return Fail(errors);

        return ApplyRefinements(result);
    }
}