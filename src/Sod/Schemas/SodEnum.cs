using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// Enum validation schema for string literals
/// </summary>
public class SodEnum<T> : SodSchema<T> where T : struct, Enum
{
    private readonly HashSet<T> _allowedValues;
    private readonly string[] _stringValues;

    public SodEnum()
    {
        _allowedValues = Enum.GetValues<T>().ToHashSet();
        _stringValues = Enum.GetNames<T>();
    }

    public SodEnum(params T[] values)
    {
        _allowedValues = values.ToHashSet();
        _stringValues = values.Select(v => v.ToString()).ToArray();
    }

    public override SodResult<T> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected an enum value, got null");
        }

        // Try direct enum value
        if (input is T enumValue && _allowedValues.Contains(enumValue))
        {
            return ApplyRefinements(enumValue);
        }

        // Try parsing from string
        if (input is string str)
        {
            if (Enum.TryParse<T>(str, true, out var parsed) && _allowedValues.Contains(parsed))
            {
                return ApplyRefinements(parsed);
            }
        }

        // Try parsing from number
        if (input is int intValue)
        {
            var enumFromInt = (T)Enum.ToObject(typeof(T), intValue);
            if (_allowedValues.Contains(enumFromInt))
            {
                return ApplyRefinements(enumFromInt);
            }
        }

        return Fail($"Invalid enum value. Expected one of: {string.Join(", ", _stringValues)}");
    }
}

/// <summary>
/// Native enum validation (allows any string from a set)
/// </summary>
public class SodNativeEnum : SodSchema<string>
{
    private readonly HashSet<string> _values;

    public SodNativeEnum(params string[] values)
    {
        _values = values.ToHashSet();
    }

    public override SodResult<string> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue!);
            if (_isOptional || _isNullable)
                return Ok(null!);
            return Fail("Expected an enum value, got null");
        }

        var str = input.ToString();
        if (str != null && _values.Contains(str))
        {
            return ApplyRefinements(str);
        }

        return Fail($"Invalid value. Expected one of: {string.Join(", ", _values.Select(v => $"'{v}'"))}");
    }
}

/// <summary>
/// Literal value validation schema
/// </summary>
public class SodLiteral<T> : SodSchema<T>
{
    private readonly T _value;
    private readonly IEqualityComparer<T> _comparer;

    public SodLiteral(T value, IEqualityComparer<T>? comparer = null)
    {
        _value = value;
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    public override SodResult<T> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_value == null)
                return ApplyRefinements(default!);
            return Fail($"Expected literal value '{_value}', got null");
        }

        if (input is T typedValue && _comparer.Equals(typedValue, _value))
        {
            return ApplyRefinements(typedValue);
        }

        // Try to convert and compare
        try
        {
            var converted = (T)Convert.ChangeType(input, typeof(T));
            if (_comparer.Equals(converted, _value))
            {
                return ApplyRefinements(converted);
            }
        }
        catch
        {
            // Conversion failed
        }

        return Fail($"Expected literal value '{_value}', got '{input}'");
    }
}