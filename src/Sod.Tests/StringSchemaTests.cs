using FluentAssertions;
using Xunit;

namespace Sod.Tests;

public class StringSchemaTests
{
    [Fact]
    public void String_Parse_ValidString_ReturnsSuccess()
    {
        var schema = Sod.String();
        var result = schema.Parse("hello");
        
        result.Success.Should().BeTrue();
        result.Data.Should().Be("hello");
    }

    [Fact]
    public void String_Parse_NonString_ReturnsFail()
    {
        var schema = Sod.String();
        var result = schema.Parse(123);
        
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Expected a string");
    }

    [Fact]
    public void String_MinLength_ValidatesCorrectly()
    {
        var schema = Sod.String().Min(5);
        
        schema.Parse("hello").Success.Should().BeTrue();
        schema.Parse("hi").Success.Should().BeFalse();
    }

    [Fact]
    public void String_MaxLength_ValidatesCorrectly()
    {
        var schema = Sod.String().Max(5);
        
        schema.Parse("hello").Success.Should().BeTrue();
        schema.Parse("hello world").Success.Should().BeFalse();
    }

    [Fact]
    public void String_Email_ValidatesCorrectly()
    {
        var schema = Sod.String().Email();
        
        schema.Parse("test@example.com").Success.Should().BeTrue();
        schema.Parse("invalid-email").Success.Should().BeFalse();
    }

    [Fact]
    public void String_Url_ValidatesCorrectly()
    {
        var schema = Sod.String().Url();
        
        schema.Parse("https://example.com").Success.Should().BeTrue();
        schema.Parse("http://localhost:3000").Success.Should().BeTrue();
        schema.Parse("not-a-url").Success.Should().BeFalse();
    }

    [Fact]
    public void String_Uuid_ValidatesCorrectly()
    {
        var schema = Sod.String().Uuid();
        
        schema.Parse("550e8400-e29b-41d4-a716-446655440000").Success.Should().BeTrue();
        schema.Parse("invalid-uuid").Success.Should().BeFalse();
    }

    [Fact]
    public void String_Regex_ValidatesCorrectly()
    {
        var schema = Sod.String().Regex(@"^\d{3}-\d{3}-\d{4}$", "Invalid phone number");
        
        schema.Parse("123-456-7890").Success.Should().BeTrue();
        var result = schema.Parse("1234567890");
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Invalid phone number");
    }

    [Fact]
    public void String_Transform_AppliesCorrectly()
    {
        var schema = Sod.String().Trim().ToUpperCase();
        
        var result = schema.Parse("  hello world  ");
        result.Success.Should().BeTrue();
        result.Data.Should().Be("HELLO WORLD");
    }

    [Fact]
    public void String_Optional_HandlesNullCorrectly()
    {
        var schema = Sod.String().Optional();
        
        var result = schema.Parse(null);
        result.Success.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public void String_Default_ProvidesDefaultValue()
    {
        var schema = Sod.String().Default("default value");
        
        var result = schema.Parse(null);
        result.Success.Should().BeTrue();
        result.Data.Should().Be("default value");
    }

    [Fact]
    public void String_Refine_CustomValidation()
    {
        var schema = Sod.String()
            .Refine(s => s.Contains("@"), "Must contain @");
        
        schema.Parse("test@example").Success.Should().BeTrue();
        var result = schema.Parse("test");
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Must contain @");
    }

    [Fact]
    public void String_StartsWith_ValidatesCorrectly()
    {
        var schema = Sod.String().StartsWith("http");
        
        schema.Parse("https://example.com").Success.Should().BeTrue();
        schema.Parse("ftp://example.com").Success.Should().BeFalse();
    }

    [Fact]
    public void String_NonEmpty_ValidatesCorrectly()
    {
        var schema = Sod.String().NonEmpty();
        
        schema.Parse("text").Success.Should().BeTrue();
        schema.Parse("").Success.Should().BeFalse();
        schema.Parse("   ").Success.Should().BeFalse();
    }
}