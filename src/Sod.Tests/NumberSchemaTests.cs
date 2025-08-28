using FluentAssertions;
using Xunit;

namespace Sod.Tests;

public class NumberSchemaTests
{
    [Fact]
    public void Number_Parse_ValidNumber_ReturnsSuccess()
    {
        var schema = Sod.Number();
        var result = schema.Parse(42);
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be(42);
    }

    [Fact]
    public void Number_Parse_StringNumber_ReturnsSuccess()
    {
        var schema = Sod.Number();
        var result = schema.Parse("42");
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be(42);
    }

    [Fact]
    public void Number_MinMax_ValidatesCorrectly()
    {
        var schema = Sod.Number().Min(10).Max(20);
        
        schema.Parse(15).Success.Should().BeTrue();
        schema.Parse(5).Success.Should().BeFalse();
        schema.Parse(25).Success.Should().BeFalse();
    }

    [Fact]
    public void Number_Positive_ValidatesCorrectly()
    {
        var schema = Sod.Number().Positive();
        
        schema.Parse(1).Success.Should().BeTrue();
        schema.Parse(0).Success.Should().BeFalse();
        schema.Parse(-1).Success.Should().BeFalse();
    }

    [Fact]
    public void Number_MultipleOf_ValidatesCorrectly()
    {
        var schema = Sod.Number().MultipleOf(5);
        
        schema.Parse(10).Success.Should().BeTrue();
        schema.Parse(15).Success.Should().BeTrue();
        schema.Parse(12).Success.Should().BeFalse();
    }

    [Fact]
    public void Float_Parse_ValidFloat_ReturnsSuccess()
    {
        var schema = Sod.Float();
        var result = schema.Parse(3.14f);
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be(3.14f);
    }

    [Fact]
    public void Decimal_Parse_ValidDecimal_ReturnsSuccess()
    {
        var schema = Sod.Decimal();
        var result = schema.Parse(123.45m);
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be(123.45m);
    }

    [Fact]
    public void Decimal_MinMax_ValidatesCorrectly()
    {
        var schema = Sod.Decimal().Min(100m).Max(200m);
        
        schema.Parse(150m).Success.Should().BeTrue();
        schema.Parse(99.99m).Success.Should().BeFalse();
        schema.Parse(200.01m).Success.Should().BeFalse();
    }

    [Fact]
    public void Number_Coerce_ConvertsTypes()
    {
        var schema = Sod.Coerce.Number();
        
        schema.Parse("42").Data.Should().Be(42);
        schema.Parse(42.6).Data.Should().Be(43); // Rounds up
        schema.Parse(42.4).Data.Should().Be(42); // Rounds down
        schema.Parse(true).Data.Should().Be(1);
        schema.Parse(false).Data.Should().Be(0);
    }

    [Fact]
    public void Number_NonNegative_ValidatesCorrectly()
    {
        var schema = Sod.Number().NonNegative();
        
        schema.Parse(0).Success.Should().BeTrue();
        schema.Parse(1).Success.Should().BeTrue();
        schema.Parse(-1).Success.Should().BeFalse();
    }

    [Fact]
    public void Float_Finite_ValidatesCorrectly()
    {
        var schema = Sod.Float().Finite();
        
        schema.Parse(3.14f).Success.Should().BeTrue();
        schema.Parse(float.PositiveInfinity).Success.Should().BeFalse();
        schema.Parse(float.NegativeInfinity).Success.Should().BeFalse();
    }
}