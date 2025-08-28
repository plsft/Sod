using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// Integer number validation schema
/// </summary>
public class SodNumber : SodSchema<int>
{
    private int? _min;
    private int? _max;
    private int? _multipleOf;
    private bool _positive;
    private bool _negative;
    private bool _nonPositive;
    private bool _nonNegative;
    private bool _finite = true;

    public SodNumber Min(int minValue)
    {
        _min = minValue;
        return this;
    }

    public SodNumber Max(int maxValue)
    {
        _max = maxValue;
        return this;
    }

    public SodNumber Gte(int value) => Min(value);
    public SodNumber Gt(int value) => Min(value + 1);
    public SodNumber Lte(int value) => Max(value);
    public SodNumber Lt(int value) => Max(value - 1);

    public SodNumber Positive()
    {
        _positive = true;
        return this;
    }

    public SodNumber Negative()
    {
        _negative = true;
        return this;
    }

    public SodNumber NonPositive()
    {
        _nonPositive = true;
        return this;
    }

    public SodNumber NonNegative()
    {
        _nonNegative = true;
        return this;
    }

    public SodNumber MultipleOf(int value)
    {
        _multipleOf = value;
        return this;
    }

    public SodNumber Int() => this; // For compatibility, already integer

    public SodNumber Finite()
    {
        _finite = true;
        return this;
    }

    public override SodResult<int> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a number, got null");
        }

        if (!int.TryParse(input.ToString(), out var num))
        {
            // Try coercion from common types
            if (input is double d && d == Math.Floor(d) && d >= int.MinValue && d <= int.MaxValue)
                num = (int)d;
            else if (input is float f && f == Math.Floor(f) && f >= int.MinValue && f <= int.MaxValue)
                num = (int)f;
            else if (input is decimal dec && dec == Math.Floor(dec) && dec >= int.MinValue && dec <= int.MaxValue)
                num = (int)dec;
            else
                return Fail($"Expected an integer, got {input}");
        }

        // Validations
        if (_positive && num <= 0)
            return Fail("Number must be positive");

        if (_negative && num >= 0)
            return Fail("Number must be negative");

        if (_nonPositive && num > 0)
            return Fail("Number must be non-positive");

        if (_nonNegative && num < 0)
            return Fail("Number must be non-negative");

        if (_min.HasValue && num < _min.Value)
            return Fail($"Number must be at least {_min.Value}");

        if (_max.HasValue && num > _max.Value)
            return Fail($"Number must be at most {_max.Value}");

        if (_multipleOf.HasValue && num % _multipleOf.Value != 0)
            return Fail($"Number must be a multiple of {_multipleOf.Value}");

        return ApplyRefinements(num);
    }
}

/// <summary>
/// Floating-point number validation schema
/// </summary>
public class SodFloat : SodSchema<float>
{
    private float? _min;
    private float? _max;
    private bool _positive;
    private bool _negative;
    private bool _nonPositive;
    private bool _nonNegative;
    private bool _finite = true;

    public SodFloat Min(float minValue)
    {
        _min = minValue;
        return this;
    }

    public SodFloat Max(float maxValue)
    {
        _max = maxValue;
        return this;
    }

    public SodFloat Gte(float value) => Min(value);
    public SodFloat Gt(float value)
    {
        _min = value;
        Refine(v => v > value, $"Number must be greater than {value}");
        return this;
    }

    public SodFloat Lte(float value) => Max(value);
    public SodFloat Lt(float value)
    {
        _max = value;
        Refine(v => v < value, $"Number must be less than {value}");
        return this;
    }

    public SodFloat Positive()
    {
        _positive = true;
        return this;
    }

    public SodFloat Negative()
    {
        _negative = true;
        return this;
    }

    public SodFloat NonPositive()
    {
        _nonPositive = true;
        return this;
    }

    public SodFloat NonNegative()
    {
        _nonNegative = true;
        return this;
    }

    public SodFloat Finite()
    {
        _finite = true;
        return this;
    }

    public override SodResult<float> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a number, got null");
        }

        if (!float.TryParse(input.ToString(), out var num))
            return Fail($"Expected a floating-point number, got {input}");

        if (_finite && !float.IsFinite(num))
            return Fail("Number must be finite");

        if (_positive && num <= 0)
            return Fail("Number must be positive");

        if (_negative && num >= 0)
            return Fail("Number must be negative");

        if (_nonPositive && num > 0)
            return Fail("Number must be non-positive");

        if (_nonNegative && num < 0)
            return Fail("Number must be non-negative");

        if (_min.HasValue && num < _min.Value)
            return Fail($"Number must be at least {_min.Value}");

        if (_max.HasValue && num > _max.Value)
            return Fail($"Number must be at most {_max.Value}");

        return ApplyRefinements(num);
    }
}

/// <summary>
/// Decimal number validation schema
/// </summary>
public class SodDecimal : SodSchema<decimal>
{
    private decimal? _min;
    private decimal? _max;
    private decimal? _multipleOf;
    private bool _positive;
    private bool _negative;
    private bool _nonPositive;
    private bool _nonNegative;

    public SodDecimal Min(decimal minValue)
    {
        _min = minValue;
        return this;
    }

    public SodDecimal Max(decimal maxValue)
    {
        _max = maxValue;
        return this;
    }

    public SodDecimal Gte(decimal value) => Min(value);
    public SodDecimal Gt(decimal value)
    {
        _min = value;
        Refine(v => v > value, $"Number must be greater than {value}");
        return this;
    }

    public SodDecimal Lte(decimal value) => Max(value);
    public SodDecimal Lt(decimal value)
    {
        _max = value;
        Refine(v => v < value, $"Number must be less than {value}");
        return this;
    }

    public SodDecimal Positive()
    {
        _positive = true;
        return this;
    }

    public SodDecimal Negative()
    {
        _negative = true;
        return this;
    }

    public SodDecimal NonPositive()
    {
        _nonPositive = true;
        return this;
    }

    public SodDecimal NonNegative()
    {
        _nonNegative = true;
        return this;
    }

    public SodDecimal MultipleOf(decimal value)
    {
        _multipleOf = value;
        return this;
    }

    public override SodResult<decimal> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a number, got null");
        }

        if (!decimal.TryParse(input.ToString(), out var num))
            return Fail($"Expected a decimal number, got {input}");

        if (_positive && num <= 0)
            return Fail("Number must be positive");

        if (_negative && num >= 0)
            return Fail("Number must be negative");

        if (_nonPositive && num > 0)
            return Fail("Number must be non-positive");

        if (_nonNegative && num < 0)
            return Fail("Number must be non-negative");

        if (_min.HasValue && num < _min.Value)
            return Fail($"Number must be at least {_min.Value}");

        if (_max.HasValue && num > _max.Value)
            return Fail($"Number must be at most {_max.Value}");

        if (_multipleOf.HasValue && num % _multipleOf.Value != 0)
            return Fail($"Number must be a multiple of {_multipleOf.Value}");

        return ApplyRefinements(num);
    }
}