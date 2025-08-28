using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// Boolean validation schema
/// </summary>
public class SodBoolean : SodSchema<bool>
{
    private bool _coerce;

    /// <summary>
    /// Enables coercion from truthy/falsy values
    /// </summary>
    public SodBoolean Coerce()
    {
        _coerce = true;
        return this;
    }

    public override SodResult<bool> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue);
            if (_isOptional || _isNullable)
                return Ok(default);
            return Fail("Expected a boolean, got null");
        }

        if (input is bool b)
            return ApplyRefinements(b);

        if (_coerce)
        {
            // Coerce from common truthy/falsy values
            var stringValue = input.ToString()?.ToLowerInvariant();
            
            if (stringValue == "true" || stringValue == "1" || stringValue == "yes" || stringValue == "on")
                return ApplyRefinements(true);
            
            if (stringValue == "false" || stringValue == "0" || stringValue == "no" || stringValue == "off" || stringValue == "")
                return ApplyRefinements(false);
            
            if (input is int i)
                return ApplyRefinements(i != 0);
        }

        return Fail($"Expected a boolean, got {input?.GetType().Name ?? "null"}");
    }
}