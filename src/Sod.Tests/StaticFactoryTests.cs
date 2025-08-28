using Xunit;
using Sod;

namespace Sod.Tests;

public class StaticFactoryTests
{
    [Fact]
    public void String_StaticFactory_CreatesValidSchema()
    {
        var schema = Sod.String();
        var result = schema.Parse("test");
        Assert.True(result.Success);
        Assert.Equal("test", result.Data);
    }

    [Fact]
    public void Number_StaticFactory_CreatesValidSchema()
    {
        var schema = Sod.Number();
        var result = schema.Parse("42");
        Assert.True(result.Success);
        Assert.Equal(42, result.Data);
    }

    [Fact] 
    public void Object_StaticFactory_CreatesValidSchema()
    {
        var schema = Sod.Object<TestUser>()
            .Field(u => u.Name, Sod.String())
            .Field(u => u.Age, Sod.Number());

        var result = schema.Parse(new Dictionary<string, object>
        {
            { "Name", "John" },
            { "Age", 30 }
        });

        Assert.True(result.Success);
        Assert.Equal("John", result.Data.Name);
        Assert.Equal(30, result.Data.Age);
    }

    [Fact]
    public void Array_StaticFactory_CreatesValidSchema()
    {
        var schema = Sod.Array(Sod.String());
        var result = schema.Parse(new[] { "a", "b", "c" });
        Assert.True(result.Success);
        Assert.Equal(3, result.Data.Count);
    }

    [Fact]
    public void Union_StaticFactory_CreatesValidSchema()
    {
        var schema = Sod.Union(Sod.String(), Sod.String());
        var result = schema.Parse("hello");
        
        Assert.True(result.Success);
    }

    public class TestUser
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }
}