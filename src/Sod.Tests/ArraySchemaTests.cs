using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Sod.Tests;

public class ArraySchemaTests
{
    [Fact]
    public void Array_Parse_ValidArray_ReturnsSuccess()
    {
        var schema = Sod.Array(Sod.Number());
        var input = new List<object> { 1, 2, 3 };

        var result = schema.Parse(input);
        
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Array_Parse_InvalidElement_ReturnsFail()
    {
        var schema = Sod.Array(Sod.Number());
        var input = new List<object> { 1, "two", 3 };

        var result = schema.Parse(input);
        
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("[1]:");
    }

    [Fact]
    public void Array_MinLength_ValidatesCorrectly()
    {
        var schema = Sod.Array(Sod.Number()).Min(3);
        
        schema.Parse(new List<object> { 1, 2, 3 }).Success.Should().BeTrue();
        schema.Parse(new List<object> { 1, 2 }).Success.Should().BeFalse();
    }

    [Fact]
    public void Array_MaxLength_ValidatesCorrectly()
    {
        var schema = Sod.Array(Sod.Number()).Max(3);
        
        schema.Parse(new List<object> { 1, 2, 3 }).Success.Should().BeTrue();
        schema.Parse(new List<object> { 1, 2, 3, 4 }).Success.Should().BeFalse();
    }

    [Fact]
    public void Array_NonEmpty_ValidatesCorrectly()
    {
        var schema = Sod.Array(Sod.String()).NonEmpty();
        
        schema.Parse(new List<object> { "a" }).Success.Should().BeTrue();
        schema.Parse(new List<object>()).Success.Should().BeFalse();
    }

    [Fact]
    public void Set_Parse_RemovesDuplicates()
    {
        var schema = Sod.Set(Sod.Number());
        var input = new List<object> { 1, 2, 2, 3, 3, 3 };

        var result = schema.Parse(input);
        
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        result.Data!.Count.Should().Be(3);
    }

    [Fact]
    public void Tuple_Parse_ValidTuple_ReturnsSuccess()
    {
        var schema = Sod.Tuple(Sod.String(), Sod.Number());
        var input = new List<object> { "hello", 42 };

        var result = schema.Parse(input);
        
        result.Success.Should().BeTrue();
        result.Data.Item1.Should().Be("hello");
        result.Data.Item2.Should().Be(42);
    }

    [Fact]
    public void Tuple_Parse_WrongLength_ReturnsFail()
    {
        var schema = Sod.Tuple(Sod.String(), Sod.Number());
        var input = new List<object> { "hello" };

        var result = schema.Parse(input);
        
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("exactly 2 elements");
    }
}