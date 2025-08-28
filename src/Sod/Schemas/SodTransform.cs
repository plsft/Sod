using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// Transform schema that applies a transformation to the parsed value
/// </summary>
public class SodTransform<TInput, TOutput> : SodSchema<TOutput>
{
    private readonly SodSchema<TInput> _inputSchema;
    private readonly Func<TInput, TOutput> _transformer;

    public SodTransform(SodSchema<TInput> inputSchema, Func<TInput, TOutput> transform)
    {
        _inputSchema = inputSchema;
        _transformer = transform;
    }

    public override SodResult<TOutput> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        var result = _inputSchema.Parse(input);
        if (!result.Success)
            return new SodResult<TOutput>(false, default, result.Errors);

        try
        {
            var transformed = _transformer(result.Data!);
            return ApplyRefinements(transformed);
        }
        catch (Exception ex)
        {
            return Fail($"Transform failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Lazy schema for recursive/circular references
/// </summary>
public class SodLazy<T> : SodSchema<T>
{
    private readonly Lazy<SodSchema<T>> _schemaFactory;

    public SodLazy(Func<SodSchema<T>> schemaFactory)
    {
        _schemaFactory = new Lazy<SodSchema<T>>(schemaFactory);
    }

    public override SodResult<T> Parse(object? input)
    {
        return _schemaFactory.Value.Parse(input);
    }
}

/// <summary>
/// Pipeline schema that chains multiple validations
/// </summary>
public class SodPipeline<T> : SodSchema<T>
{
    private readonly List<SodSchema<T>> _pipeline = new();

    public SodPipeline(params SodSchema<T>[] schemas)
    {
        _pipeline.AddRange(schemas);
    }

    public SodPipeline<T> Pipe(SodSchema<T> schema)
    {
        _pipeline.Add(schema);
        return this;
    }

    public override SodResult<T> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        T? currentValue = default;
        
        foreach (var schema in _pipeline)
        {
            var result = schema.Parse(input);
            if (!result.Success)
                return result;
            
            currentValue = result.Data;
            input = currentValue; // Pass the result to the next schema
        }

        return ApplyRefinements(currentValue!);
    }
}

/// <summary>
/// Coerce schema that attempts to coerce values to the target type
/// </summary>
public class SodCoerce
{
    public static SodString String()
    {
        var schema = new SodString();
        schema.Preprocess(input =>
        {
            if (input == null) return null!;
            return input.ToString()!;
        });
        return schema;
    }

    public static SodNumber Number()
    {
        var schema = new SodNumber();
        schema.Preprocess(input =>
        {
            if (input == null) return null!;
            
            // Handle numeric types directly
            if (input is double d)
                return (int)Math.Round(d);
            if (input is float f)
                return (int)Math.Round(f);
            if (input is decimal dec)
                return (int)Math.Round(dec);
            if (input is bool b)
                return b ? 1 : 0;
            
            // Try parsing from string
            var str = input.ToString();
            if (int.TryParse(str, out var result))
                return result;
            
            if (double.TryParse(str, out var parsed))
                return (int)Math.Round(parsed);
            
            return input;
        });
        return schema;
    }

    public static SodBoolean Boolean()
    {
        return new SodBoolean().Coerce();
    }

    public static SodDate Date()
    {
        return new SodDate().Coerce();
    }

    public static SodDecimal Decimal()
    {
        var schema = new SodDecimal();
        schema.Preprocess(input =>
        {
            if (input == null) return null!;
            
            if (decimal.TryParse(input.ToString(), out var result))
                return result;
            
            return input;
        });
        return schema;
    }

    public static SodFloat Float()
    {
        var schema = new SodFloat();
        schema.Preprocess(input =>
        {
            if (input == null) return null!;
            
            if (float.TryParse(input.ToString(), out var result))
                return result;
            
            return input;
        });
        return schema;
    }
}

/// <summary>
/// Branded type schema for nominal typing
/// </summary>
public class SodBrand<T, TBrand> : SodSchema<T>
{
    private readonly SodSchema<T> _baseSchema;
    private readonly string _brandName;

    public SodBrand(SodSchema<T> baseSchema, string brandName)
    {
        _baseSchema = baseSchema;
        _brandName = brandName;
    }

    public override SodResult<T> Parse(object? input)
    {
        var result = _baseSchema.Parse(input);
        if (!result.Success)
            return result;

        // In a real implementation, you might want to wrap the value
        // in a branded type wrapper
        return ApplyRefinements(result.Data!);
    }

    public string Brand => _brandName;
}

/// <summary>
/// Catch schema that provides a fallback value on error
/// </summary>
public class SodCatch<T> : SodSchema<T>
{
    private readonly SodSchema<T> _schema;
    private readonly T _fallback;

    public SodCatch(SodSchema<T> schema, T fallback)
    {
        _schema = schema;
        _fallback = fallback;
    }

    public override SodResult<T> Parse(object? input)
    {
        try
        {
            var result = _schema.Parse(input);
            if (result.Success)
                return result;
            
            return Ok(_fallback);
        }
        catch
        {
            return Ok(_fallback);
        }
    }
}