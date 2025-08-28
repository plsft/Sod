using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// Union type that can be one of two types
/// </summary>
public class SodOr<T1, T2> : SodSchema<object>
{
    private readonly SodSchema<T1> _schema1;
    private readonly SodSchema<T2> _schema2;

    public SodOr(SodSchema<T1> schema1, SodSchema<T2> schema2)
    {
        _schema1 = schema1;
        _schema2 = schema2;
    }

    public override SodResult<object> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        var result1 = _schema1.Parse(input);
        if (result1.Success)
            return ApplyRefinements(result1.Data!);

        var result2 = _schema2.Parse(input);
        if (result2.Success)
            return ApplyRefinements(result2.Data!);

        return Fail($"Value does not match any schema. Errors: {result1.Error} OR {result2.Error}");
    }
}