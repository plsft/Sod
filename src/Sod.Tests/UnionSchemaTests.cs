using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Sod.Core;
using Sod.Schemas;

namespace Sod.Tests;

public class UnionSchemaTests
{
    [Fact]
    public void Union_Parse_MatchesFirstSchema_ReturnsSuccess()
    {
        var schema = new SodOr<string, int>(
            Sod.String(),
            Sod.Number()
        );

        var result = schema.Parse("hello");
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be("hello");
    }

    [Fact]
    public void Union_Parse_MatchesSecondSchema_ReturnsSuccess()
    {
        var schema = new SodOr<string, int>(
            Sod.String(),
            Sod.Number()
        );

        var result = schema.Parse(42);
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be(42);
    }

    [Fact]
    public void Union_Parse_NoMatch_ReturnsFail()
    {
        var schema = new SodOr<string, int>(
            Sod.String(),
            Sod.Number()
        );

        var result = schema.Parse(true);
        
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("does not match any schema");
    }

    [Fact]
    public void DiscriminatedUnion_Parse_CorrectDiscriminator_ReturnsSuccess()
    {
        var schema = Sod.DiscriminatedUnion<object>("type")
            .Option("string", Sod.Any())
            .Option("number", Sod.Any());

        var input = new Dictionary<string, object>
        {
            { "type", "string" },
            { "value", "hello" }
        };

        var result = schema.Parse(input);
        
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void DiscriminatedUnion_Parse_InvalidDiscriminator_ReturnsFail()
    {
        var schema = Sod.DiscriminatedUnion<object>("type")
            .Option("string", Sod.Any())
            .Option("number", Sod.Any());

        var input = new Dictionary<string, object>
        {
            { "type", "boolean" },
            { "value", true }
        };

        var result = schema.Parse(input);
        
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Invalid discriminator value");
    }

    [Fact]
    public void Intersection_Parse_SatisfiesAllSchemas_ReturnsSuccess()
    {
        var schema = Sod.Intersection<string>(
            Sod.String().Min(5),
            Sod.String().Max(10)
        );

        var result = schema.Parse("hello");
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be("hello");
    }

    [Fact]
    public void Intersection_Parse_FailsOneSchema_ReturnsFail()
    {
        var schema = Sod.Intersection<string>(
            Sod.String().Min(5),
            Sod.String().Max(10)
        );

        var result = schema.Parse("hi");
        
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("at least 5");
    }
}