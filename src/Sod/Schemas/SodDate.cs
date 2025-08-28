using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// DateTime validation schema
/// </summary>
public class SodDate : SodSchema<DateTime>
{
    private DateTime? _min;
    private DateTime? _max;
    private bool _coerce;

    public SodDate Min(DateTime minDate)
    {
        _min = minDate;
        return this;
    }

    public SodDate Max(DateTime maxDate)
    {
        _max = maxDate;
        return this;
    }

    /// <summary>
    /// Enables coercion from string and numeric timestamps
    /// </summary>
    public SodDate Coerce()
    {
        _coerce = true;
        return this;
    }

    public override SodResult<DateTime> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a date, got null");
        }

        DateTime date;

        if (input is DateTime dt)
        {
            date = dt;
        }
        else if (input is DateTimeOffset dto)
        {
            date = dto.DateTime;
        }
        else if (_coerce)
        {
            // Try to coerce from string
            if (DateTime.TryParse(input.ToString(), out date))
            {
                // Successfully parsed
            }
            // Try to coerce from Unix timestamp (milliseconds)
            else if (input is long timestamp)
            {
                try
                {
                    date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                }
                catch
                {
                    return Fail($"Invalid timestamp: {input}");
                }
            }
            // Try to coerce from Unix timestamp (seconds)
            else if (input is int timestampSeconds)
            {
                try
                {
                    date = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds).DateTime;
                }
                catch
                {
                    return Fail($"Invalid timestamp: {input}");
                }
            }
            else
            {
                return Fail($"Cannot coerce {input} to DateTime");
            }
        }
        else if (!DateTime.TryParse(input.ToString(), out date))
        {
            return Fail($"Expected a valid date, got {input}");
        }

        // Validations
        if (_min.HasValue && date < _min.Value)
            return Fail($"Date must be after {_min.Value:yyyy-MM-dd}");

        if (_max.HasValue && date > _max.Value)
            return Fail($"Date must be before {_max.Value:yyyy-MM-dd}");

        return ApplyRefinements(date);
    }
}

/// <summary>
/// TimeSpan validation schema
/// </summary>
public class SodTime : SodSchema<TimeSpan>
{
    private TimeSpan? _min;
    private TimeSpan? _max;

    public SodTime Min(TimeSpan minTime)
    {
        _min = minTime;
        return this;
    }

    public SodTime Max(TimeSpan maxTime)
    {
        _max = maxTime;
        return this;
    }

    public override SodResult<TimeSpan> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a time, got null");
        }

        TimeSpan time;

        if (input is TimeSpan ts)
        {
            time = ts;
        }
        else if (!TimeSpan.TryParse(input.ToString(), out time))
        {
            return Fail($"Expected a valid time, got {input}");
        }

        // Validations
        if (_min.HasValue && time < _min.Value)
            return Fail($"Time must be at least {_min.Value}");

        if (_max.HasValue && time > _max.Value)
            return Fail($"Time must be at most {_max.Value}");

        return ApplyRefinements(time);
    }
}

/// <summary>
/// DateOnly validation schema for .NET 6+
/// </summary>
public class SodDateOnly : SodSchema<DateOnly>
{
    private DateOnly? _min;
    private DateOnly? _max;

    public SodDateOnly Min(DateOnly minDate)
    {
        _min = minDate;
        return this;
    }

    public SodDateOnly Max(DateOnly maxDate)
    {
        _max = maxDate;
        return this;
    }

    public override SodResult<DateOnly> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a date, got null");
        }

        DateOnly date;

        if (input is DateOnly d)
        {
            date = d;
        }
        else if (input is DateTime dt)
        {
            date = DateOnly.FromDateTime(dt);
        }
        else if (!DateOnly.TryParse(input.ToString(), out date))
        {
            return Fail($"Expected a valid date, got {input}");
        }

        // Validations
        if (_min.HasValue && date < _min.Value)
            return Fail($"Date must be after {_min.Value:yyyy-MM-dd}");

        if (_max.HasValue && date > _max.Value)
            return Fail($"Date must be before {_max.Value:yyyy-MM-dd}");

        return ApplyRefinements(date);
    }
}

/// <summary>
/// TimeOnly validation schema for .NET 6+
/// </summary>
public class SodTimeOnly : SodSchema<TimeOnly>
{
    private TimeOnly? _min;
    private TimeOnly? _max;

    public SodTimeOnly Min(TimeOnly minTime)
    {
        _min = minTime;
        return this;
    }

    public SodTimeOnly Max(TimeOnly maxTime)
    {
        _max = maxTime;
        return this;
    }

    public override SodResult<TimeOnly> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a time, got null");
        }

        TimeOnly time;

        if (input is TimeOnly t)
        {
            time = t;
        }
        else if (input is TimeSpan ts)
        {
            time = TimeOnly.FromTimeSpan(ts);
        }
        else if (!TimeOnly.TryParse(input.ToString(), out time))
        {
            return Fail($"Expected a valid time, got {input}");
        }

        // Validations
        if (_min.HasValue && time < _min.Value)
            return Fail($"Time must be after {_min.Value}");

        if (_max.HasValue && time > _max.Value)
            return Fail($"Time must be before {_max.Value}");

        return ApplyRefinements(time);
    }
}