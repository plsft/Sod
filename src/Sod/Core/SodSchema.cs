using System.Linq.Expressions;
using Sod.Schemas;

namespace Sod.Core;

/// <summary>
/// Base class for all Sod schema validators
/// </summary>
public abstract class SodSchema<T>
{
    public string? _description;
    public readonly List<Func<T, ValidationContext, SodResult<T>>> _refinements = new();
    public T? _defaultValue;
    public bool _hasDefault;
    public bool _isOptional;
    public bool _isNullable;
    public Func<object, object>? _preprocess;

    /// <summary>
    /// Parses and validates the input
    /// </summary>
    public abstract SodResult<T> Parse(object? input);

    /// <summary>
    /// Safely parses the input without throwing exceptions
    /// </summary>
    public SodResult<T> SafeParse(object? input)
    {
        try
        {
            return Parse(input);
        }
        catch (Exception ex)
        {
            return Fail($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses the input or throws an exception if validation fails
    /// </summary>
    public T ParseOrThrow(object? input)
    {
        var result = Parse(input);
        if (!result.Success)
            throw new SodValidationException(result.Errors);
        return result.Data!;
    }

    /// <summary>
    /// Adds a description to the schema
    /// </summary>
    public SodSchema<T> Describe(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Makes the field optional (can be undefined)
    /// </summary>
    public SodSchema<T?> Optional()
    {
        _isOptional = true;
        return this as SodSchema<T?> ?? throw new InvalidOperationException("Cannot make schema optional");
    }

    /// <summary>
    /// Makes the field nullable
    /// </summary>
    public SodSchema<T?> Nullable()
    {
        _isNullable = true;
        return this as SodSchema<T?> ?? throw new InvalidOperationException("Cannot make schema nullable");
    }

    /// <summary>
    /// Sets a default value for when the input is null or undefined
    /// </summary>
    public SodSchema<T> Default(T defaultValue)
    {
        _defaultValue = defaultValue;
        _hasDefault = true;
        return this;
    }

    /// <summary>
    /// Adds a custom refinement validation
    /// </summary>
    public SodSchema<T> Refine(Func<T, bool> validation, string message)
    {
        _refinements.Add((value, context) =>
        {
            if (!validation(value))
                return Fail(message);
            return Ok(value);
        });
        return this;
    }

    /// <summary>
    /// Adds a transformation to the output
    /// </summary>
    public SodSchema<TOutput> Transform<TOutput>(Func<T, TOutput> transform)
    {
        return new SodTransform<T, TOutput>(this, transform);
    }

    /// <summary>
    /// Preprocesses the input before validation
    /// </summary>
    public SodSchema<T> Preprocess(Func<object, object> preprocessor)
    {
        _preprocess = preprocessor;
        return this;
    }

    protected SodResult<T> Ok(T value) => new SodResult<T>(true, value, Array.Empty<string>());

    protected SodResult<T> Fail(string error) => new SodResult<T>(false, default, new[] { error });

    protected SodResult<T> Fail(IEnumerable<string> errors) => new SodResult<T>(false, default, errors.ToArray());

    protected SodResult<T> ApplyRefinements(T value, ValidationContext? context = null)
    {
        context ??= new ValidationContext();
        
        foreach (var refinement in _refinements)
        {
            var result = refinement(value, context);
            if (!result.Success)
                return result;
        }
        
        return Ok(value);
    }

    protected object? ApplyPreprocess(object? input)
    {
        return _preprocess != null ? _preprocess(input) : input;
    }
}

/// <summary>
/// Validation context for complex validations
/// </summary>
public class ValidationContext
{
    public string Path { get; set; } = "";
    public Dictionary<string, object> Data { get; } = new();
}

/// <summary>
/// Result of a validation operation
/// </summary>
public class SodResult<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public string[] Errors { get; }
    public string Error => string.Join("; ", Errors);

    public SodResult(bool success, T? data, string[] errors)
    {
        Success = success;
        Data = data;
        Errors = errors ?? Array.Empty<string>();
    }

    public SodResult<TOutput> Map<TOutput>(Func<T, TOutput> mapper)
    {
        if (!Success)
            return new SodResult<TOutput>(false, default, Errors);
        
        return new SodResult<TOutput>(true, mapper(Data!), Array.Empty<string>());
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class SodValidationException : Exception
{
    public string[] Errors { get; }

    public SodValidationException(string[] errors) 
        : base($"Validation failed: {string.Join("; ", errors)}")
    {
        Errors = errors;
    }
}