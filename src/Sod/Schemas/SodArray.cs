using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// Array/List validation schema
/// </summary>
public class SodArray<T> : SodSchema<List<T>>
{
    private readonly SodSchema<T> _elementSchema;
    private int? _minLength;
    private int? _maxLength;
    private int? _exactLength;
    private bool _nonEmpty;

    public SodArray(SodSchema<T> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    public SodArray<T> Min(int min)
    {
        _minLength = min;
        return this;
    }

    public SodArray<T> Max(int max)
    {
        _maxLength = max;
        return this;
    }

    public SodArray<T> Length(int length)
    {
        _exactLength = length;
        return this;
    }

    public SodArray<T> NonEmpty()
    {
        _nonEmpty = true;
        return this;
    }

    public override SodResult<List<T>> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue!);
            if (_isOptional || _isNullable)
                return Ok(null!);
            return Fail("Expected an array, got null");
        }

        if (input is not IEnumerable<object> collection)
        {
            // Try to handle arrays of specific types
            if (input is System.Collections.IEnumerable enumerable)
            {
                collection = enumerable.Cast<object>();
            }
            else
            {
                return Fail($"Expected an array, got {input.GetType().Name}");
            }
        }

        var results = new List<T>();
        var errors = new List<string>();
        var index = 0;

        foreach (var item in collection)
        {
            var parsed = _elementSchema.Parse(item);
            if (!parsed.Success)
            {
                errors.Add($"[{index}]: {parsed.Error}");
            }
            else
            {
                results.Add(parsed.Data!);
            }
            index++;
        }

        if (errors.Any())
            return Fail(errors);

        // Length validations
        if (_nonEmpty && results.Count == 0)
            return Fail("Array cannot be empty");

        if (_exactLength.HasValue && results.Count != _exactLength.Value)
            return Fail($"Array must have exactly {_exactLength.Value} elements");

        if (_minLength.HasValue && results.Count < _minLength.Value)
            return Fail($"Array must have at least {_minLength.Value} elements");

        if (_maxLength.HasValue && results.Count > _maxLength.Value)
            return Fail($"Array must have at most {_maxLength.Value} elements");

        return ApplyRefinements(results);
    }
}

/// <summary>
/// Set validation schema (ensures unique elements)
/// </summary>
public class SodSet<T> : SodSchema<HashSet<T>>
{
    private readonly SodSchema<T> _elementSchema;
    private int? _minSize;
    private int? _maxSize;
    private int? _exactSize;

    public SodSet(SodSchema<T> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    public SodSet<T> Min(int min)
    {
        _minSize = min;
        return this;
    }

    public SodSet<T> Max(int max)
    {
        _maxSize = max;
        return this;
    }

    public SodSet<T> Size(int size)
    {
        _exactSize = size;
        return this;
    }

    public override SodResult<HashSet<T>> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue!);
            if (_isOptional || _isNullable)
                return Ok(null!);
            return Fail("Expected a set, got null");
        }

        if (input is not IEnumerable<object> collection)
        {
            if (input is System.Collections.IEnumerable enumerable)
            {
                collection = enumerable.Cast<object>();
            }
            else
            {
                return Fail($"Expected a set, got {input.GetType().Name}");
            }
        }

        var results = new HashSet<T>();
        var errors = new List<string>();
        var index = 0;

        foreach (var item in collection)
        {
            var parsed = _elementSchema.Parse(item);
            if (!parsed.Success)
            {
                errors.Add($"[{index}]: {parsed.Error}");
            }
            else
            {
                results.Add(parsed.Data!);
            }
            index++;
        }

        if (errors.Any())
            return Fail(errors);

        // Size validations
        if (_exactSize.HasValue && results.Count != _exactSize.Value)
            return Fail($"Set must have exactly {_exactSize.Value} unique elements");

        if (_minSize.HasValue && results.Count < _minSize.Value)
            return Fail($"Set must have at least {_minSize.Value} unique elements");

        if (_maxSize.HasValue && results.Count > _maxSize.Value)
            return Fail($"Set must have at most {_maxSize.Value} unique elements");

        return ApplyRefinements(results);
    }
}

/// <summary>
/// Tuple validation schema
/// </summary>
public class SodTuple<T1, T2> : SodSchema<(T1, T2)>
{
    private readonly SodSchema<T1> _item1Schema;
    private readonly SodSchema<T2> _item2Schema;

    public SodTuple(SodSchema<T1> item1Schema, SodSchema<T2> item2Schema)
    {
        _item1Schema = item1Schema;
        _item2Schema = item2Schema;
    }

    public override SodResult<(T1, T2)> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a tuple, got null");
        }

        if (input is not IList<object> list || list.Count != 2)
        {
            return Fail("Expected a tuple with exactly 2 elements");
        }

        var result1 = _item1Schema.Parse(list[0]);
        var result2 = _item2Schema.Parse(list[1]);

        var errors = new List<string>();
        if (!result1.Success)
            errors.Add($"Item 1: {result1.Error}");
        if (!result2.Success)
            errors.Add($"Item 2: {result2.Error}");

        if (errors.Any())
            return Fail(errors);

        return ApplyRefinements((result1.Data!, result2.Data!));
    }
}

/// <summary>
/// Tuple validation schema for 3 elements
/// </summary>
public class SodTuple<T1, T2, T3> : SodSchema<(T1, T2, T3)>
{
    private readonly SodSchema<T1> _item1Schema;
    private readonly SodSchema<T2> _item2Schema;
    private readonly SodSchema<T3> _item3Schema;

    public SodTuple(SodSchema<T1> item1Schema, SodSchema<T2> item2Schema, SodSchema<T3> item3Schema)
    {
        _item1Schema = item1Schema;
        _item2Schema = item2Schema;
        _item3Schema = item3Schema;
    }

    public override SodResult<(T1, T2, T3)> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a tuple, got null");
        }

        if (input is not IList<object> list || list.Count != 3)
        {
            return Fail("Expected a tuple with exactly 3 elements");
        }

        var result1 = _item1Schema.Parse(list[0]);
        var result2 = _item2Schema.Parse(list[1]);
        var result3 = _item3Schema.Parse(list[2]);

        var errors = new List<string>();
        if (!result1.Success)
            errors.Add($"Item 1: {result1.Error}");
        if (!result2.Success)
            errors.Add($"Item 2: {result2.Error}");
        if (!result3.Success)
            errors.Add($"Item 3: {result3.Error}");

        if (errors.Any())
            return Fail(errors);

        return ApplyRefinements((result1.Data!, result2.Data!, result3.Data!));
    }
}