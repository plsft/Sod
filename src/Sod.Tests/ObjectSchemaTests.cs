using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Sod.Tests;

public class ObjectSchemaTests
{
    public class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string? Email { get; set; }
    }

    [Fact]
    public void Object_Parse_ValidObject_ReturnsSuccess()
    {
        var schema = Sod.Object<Person>()
            .Field(p => p.Name, Sod.String())
            .Field(p => p.Age, Sod.Number());

        var input = new Dictionary<string, object>
        {
            { "Name", "John" },
            { "Age", 30 }
        };

        var result = schema.Parse(input);
        
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("John");
        result.Data.Age.Should().Be(30);
    }

    [Fact]
    public void Object_Parse_MissingRequiredField_ReturnsFail()
    {
        var schema = Sod.Object<Person>()
            .Field(p => p.Name, Sod.String())
            .Field(p => p.Age, Sod.Number());

        var input = new Dictionary<string, object>
        {
            { "Name", "John" }
        };

        var result = schema.Parse(input);
        
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Missing required field 'Age'");
    }

    [Fact]
    public void Object_Parse_OptionalField_AllowsMissing()
    {
        var schema = Sod.Object<Person>()
            .Field(p => p.Name, Sod.String())
            .Field(p => p.Age, Sod.Number())
            .Field(p => p.Email, Sod.String().Optional());

        var input = new Dictionary<string, object>
        {
            { "Name", "John" },
            { "Age", 30 }
        };

        var result = schema.Parse(input);
        
        result.Success.Should().BeTrue();
        result.Data!.Email.Should().BeNull();
    }

    [Fact]
    public void Object_Strict_RejectsUnknownFields()
    {
        var schema = Sod.Object<Person>()
            .Field(p => p.Name, Sod.String())
            .Strict();

        var input = new Dictionary<string, object>
        {
            { "Name", "John" },
            { "UnknownField", "value" }
        };

        var result = schema.Parse(input);
        
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Unexpected keys");
    }

    [Fact]
    public void Object_WithValidation_ValidatesFields()
    {
        var schema = Sod.Object<Person>()
            .Field(p => p.Name, Sod.String().Min(2))
            .Field(p => p.Age, Sod.Number().Min(18).Max(100));

        var input = new Dictionary<string, object>
        {
            { "Name", "J" },
            { "Age", 150 }
        };

        var result = schema.Parse(input);
        
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("at least 2 characters");
        result.Error.Should().Contain("at most 100");
    }

    [Fact]
    public void Object_DefaultValues_AppliesDefaults()
    {
        var schema = Sod.Object<Person>()
            .Field(p => p.Name, Sod.String().Default("Unknown"))
            .Field(p => p.Age, Sod.Number().Default(0));

        var input = new Dictionary<string, object>();

        var result = schema.Parse(input);
        
        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Unknown");
        result.Data.Age.Should().Be(0);
    }

    [Fact]
    public void Object_Partial_MakesAllFieldsOptional()
    {
        var schema = Sod.Object<Person>()
            .Field(p => p.Name, Sod.String())
            .Field(p => p.Age, Sod.Number())
            .Partial();

        var input = new Dictionary<string, object>();

        var result = schema.Parse(input);
        
        result.Success.Should().BeTrue();
    }
}