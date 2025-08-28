using FluentAssertions;
using Xunit;
using Sod.Core;
using Sod.Schemas;

namespace Sod.Tests;

public class TransformSchemaTests
{
    [Fact]
    public void Transform_Parse_AppliesTransformation()
    {
        var schema = Sod.String().Transform(s => s.Length);

        var result = schema.Parse("hello");
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be(5);
    }

    [Fact]
    public void Transform_Parse_ChainedTransformations()
    {
        var schema = Sod.String()
            .Transform(s => s.ToUpper())
            .Transform(s => s.Replace("HELLO", "HI"));

        var result = schema.Parse("hello world");
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be("HI WORLD");
    }

    [Fact]
    public void Preprocess_ModifiesInputBeforeValidation()
    {
        var schema = Sod.String();
        schema.Preprocess(input => input?.ToString()?.Trim() ?? "");
        schema.Min(3);

        var result = schema.Parse("  hi  ");
        
        result.Success.Should().BeFalse(); // "hi" is less than 3 chars after trim
    }

    [Fact]
    public void Lazy_Parse_HandlesRecursiveTypes()
    {
        var lazySchema = Sod.Lazy(() => Sod.String().Min(3));

        var result = lazySchema.Parse("hello");
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be("hello");
    }

    [Fact]
    public void Pipeline_Parse_ChainsMultipleSchemas()
    {
        var schema = Sod.Pipeline(
            Sod.String(),
            Sod.String().Min(3),
            Sod.String().Max(10)
        );

        schema.Parse("hello").Success.Should().BeTrue();
        schema.Parse("hi").Success.Should().BeFalse();
        schema.Parse("this is too long").Success.Should().BeFalse();
    }

    [Fact]
    public void Catch_Parse_ReturnsFallbackOnError()
    {
        var schema = Sod.Catch(Sod.Number(), 0);

        schema.Parse(42).Data.Should().Be(42);
        schema.Parse("invalid").Data.Should().Be(0);
        schema.Parse(null).Data.Should().Be(0);
    }

    [Fact]
    public void Brand_Parse_ValidatesBrandedType()
    {
        var schema = Sod.Brand<string, object>(
            Sod.String().Email(),
            "Email"
        );

        var result = schema.Parse("test@example.com");
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be("test@example.com");
        schema.Brand.Should().Be("Email");
    }
}