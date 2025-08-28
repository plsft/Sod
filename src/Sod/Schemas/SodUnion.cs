using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// Union validation schema (validates against multiple possible schemas)
/// </summary>
public class SodUnion<T> : SodSchema<T>
{
    private readonly List<SodSchema<T>> _schemas = new();

    public SodUnion(params SodSchema<T>[] schemas)
    {
        _schemas.AddRange(schemas);
    }

    public SodUnion<T> Or(SodSchema<T> schema)
    {
        _schemas.Add(schema);
        return this;
    }

    public override SodResult<T> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        var errors = new List<string>();
        
        foreach (var schema in _schemas)
        {
            var result = schema.Parse(input);
            if (result.Success)
            {
                return ApplyRefinements(result.Data!);
            }
            errors.AddRange(result.Errors);
        }

        return Fail($"None of the union schemas matched. Errors: {string.Join(" OR ", errors)}");
    }
}

/// <summary>
/// Discriminated union validation schema
/// </summary>
public class SodDiscriminatedUnion<T> : SodSchema<T>
{
    private readonly string _discriminatorKey;
    private readonly Dictionary<string, SodSchema<T>> _schemas = new();

    public SodDiscriminatedUnion(string discriminatorKey)
    {
        _discriminatorKey = discriminatorKey;
    }

    public SodDiscriminatedUnion<T> Option(string discriminatorValue, SodSchema<T> schema)
    {
        _schemas[discriminatorValue] = schema;
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
            return Fail("Expected an object with discriminator, got null");
        }

        // Try to get discriminator value
        string? discriminatorValue = null;
        
        if (input is IDictionary<string, object> dict && dict.TryGetValue(_discriminatorKey, out var discValue))
        {
            discriminatorValue = discValue?.ToString();
        }
        else
        {
            // Try reflection for typed objects
            var type = input.GetType();
            var prop = type.GetProperty(_discriminatorKey);
            if (prop != null)
            {
                discriminatorValue = prop.GetValue(input)?.ToString();
            }
        }

        if (discriminatorValue == null)
        {
            return Fail($"Missing discriminator key '{_discriminatorKey}'");
        }

        if (!_schemas.TryGetValue(discriminatorValue, out var schema))
        {
            var validValues = string.Join(", ", _schemas.Keys.Select(k => $"'{k}'"));
            return Fail($"Invalid discriminator value '{discriminatorValue}'. Expected one of: {validValues}");
        }

        var result = schema.Parse(input);
        if (!result.Success)
            return result;

        return ApplyRefinements(result.Data!);
    }
}

/// <summary>
/// Intersection validation schema (must satisfy all schemas)
/// </summary>
public class SodIntersection<T> : SodSchema<T>
{
    private readonly List<SodSchema<T>> _schemas = new();

    public SodIntersection(params SodSchema<T>[] schemas)
    {
        _schemas.AddRange(schemas);
    }

    public SodIntersection<T> And(SodSchema<T> schema)
    {
        _schemas.Add(schema);
        return this;
    }

    public override SodResult<T> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        T? result = default;
        var errors = new List<string>();
        
        foreach (var schema in _schemas)
        {
            var parseResult = schema.Parse(input);
            if (!parseResult.Success)
            {
                errors.AddRange(parseResult.Errors);
            }
            else
            {
                result = parseResult.Data;
                // For intersection, the input must transform through each schema
                // This might need more complex merging logic for objects
                input = result;
            }
        }

        if (errors.Any())
            return Fail(errors);

        return ApplyRefinements(result!);
    }
}

/// <summary>
/// Any schema - accepts any value
/// </summary>
public class SodAny : SodSchema<object>
{
    public override SodResult<object> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue!);
            if (_isOptional || _isNullable)
                return Ok(null!);
        }

        return ApplyRefinements(input!);
    }
}

/// <summary>
/// Unknown schema - similar to any but more explicit
/// </summary>
public class SodUnknown : SodSchema<object>
{
    public override SodResult<object> Parse(object? input)
    {
        input = ApplyPreprocess(input);
        return ApplyRefinements(input!);
    }
}

/// <summary>
/// Never schema - always fails validation
/// </summary>
public class SodNever : SodSchema<object>
{
    private readonly string _message;

    public SodNever(string message = "This value should never be provided")
    {
        _message = message;
    }

    public override SodResult<object> Parse(object? input)
    {
        return Fail(_message);
    }
}

/// <summary>
/// Void schema - expects undefined/null
/// </summary>
public class SodVoid : SodSchema<object?>
{
    public override SodResult<object?> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
            return Ok(null);

        return Fail("Expected void (null), got a value");
    }
}

/// <summary>
/// Null schema - expects null
/// </summary>
public class SodNull : SodSchema<object?>
{
    public override SodResult<object?> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
            return Ok(null);

        return Fail($"Expected null, got {input.GetType().Name}");
    }
}

/// <summary>
/// Undefined schema - expects null/undefined
/// </summary>
public class SodUndefined : SodSchema<object?>
{
    public override SodResult<object?> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
            return Ok(null);

        return Fail($"Expected undefined/null, got {input.GetType().Name}");
    }
}