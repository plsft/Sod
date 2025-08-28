using Sod;
using System;

// Example showing how unions work in Sod
class UnionExample
{
    static void Main()
    {
        Console.WriteLine("=== Sod Union Examples ===\n");

        // 1. Basic Union: String OR Number
        Console.WriteLine("1. Basic Union (String OR Number):");
        var stringOrNumber = Sod.Union(Sod.String(), Sod.String()); // Same type for demo
        
        // In practice, you'd want object as the union type for different types
        var mixedUnion = Sod.Union<object>(
            Sod.String().Transform(s => (object)s),
            Sod.Number().Transform(n => (object)n)
        );

        // Test with string
        var result1 = stringOrNumber.Parse("hello");
        Console.WriteLine($"String input: Success={result1.Success}, Data={result1.Data}");
        
        // Test with another string  
        var result2 = stringOrNumber.Parse("world");
        Console.WriteLine($"String input: Success={result2.Success}, Data={result2.Data}");

        Console.WriteLine();

        // 2. Discriminated Union (like TypeScript discriminated unions)
        Console.WriteLine("2. Discriminated Union:");
        var shapeUnion = Sod.DiscriminatedUnion<Shape>("type")
            .Option("circle", Sod.Object<Circle>()
                .Field(c => c.Type, Sod.Literal("circle"))
                .Field(c => c.Radius, Sod.Number().Positive()))
            .Option("rectangle", Sod.Object<Rectangle>()
                .Field(r => r.Type, Sod.Literal("rectangle"))
                .Field(r => r.Width, Sod.Number().Positive())
                .Field(r => r.Height, Sod.Number().Positive()));

        // Test circle
        var circleData = new Dictionary<string, object>
        {
            { "type", "circle" },
            { "radius", 5.0 }
        };
        
        var circleResult = shapeUnion.Parse(circleData);
        Console.WriteLine($"Circle: Success={circleResult.Success}");
        if (circleResult.Success && circleResult.Data is Circle c)
        {
            Console.WriteLine($"  Parsed circle with radius: {c.Radius}");
        }

        // Test rectangle
        var rectangleData = new Dictionary<string, object>
        {
            { "type", "rectangle" },
            { "width", 10.0 },
            { "height", 20.0 }
        };
        
        var rectResult = shapeUnion.Parse(rectangleData);
        Console.WriteLine($"Rectangle: Success={rectResult.Success}");
        if (rectResult.Success && rectResult.Data is Rectangle r)
        {
            Console.WriteLine($"  Parsed rectangle: {r.Width}x{r.Height}");
        }

        Console.WriteLine();

        // 3. Practical Union Example: ID that can be string or number
        Console.WriteLine("3. Practical Union - Flexible ID:");
        
        // Create a union that accepts either string or number IDs
        var idSchema = Sod.Union<object>(
            Sod.String().Transform(s => (object)s),
            Sod.Number().Transform(n => (object)n.ToString()) // Convert to string
        );

        var stringId = idSchema.Parse("user-123");
        var numberId = idSchema.Parse("456");
        
        Console.WriteLine($"String ID: Success={stringId.Success}, Value={stringId.Data}");
        Console.WriteLine($"Number ID: Success={numberId.Success}, Value={numberId.Data}");

        Console.WriteLine();

        // 4. Union with validation
        Console.WriteLine("4. Union with Different Validations:");
        
        var emailOrPhone = Sod.Union(
            Sod.String().Email(),           // Must be valid email
            Sod.String().Regex(@"^\d{10}$") // Must be 10-digit phone
        );

        var emailTest = emailOrPhone.Parse("user@example.com");
        var phoneTest = emailOrPhone.Parse("1234567890");
        var invalidTest = emailOrPhone.Parse("not-valid");

        Console.WriteLine($"Email: Success={emailTest.Success}");
        Console.WriteLine($"Phone: Success={phoneTest.Success}");  
        Console.WriteLine($"Invalid: Success={invalidTest.Success}");
        if (!invalidTest.Success)
        {
            Console.WriteLine($"  Error: {invalidTest.Error}");
        }
    }
}

// Supporting classes
public abstract class Shape
{
    public string Type { get; set; } = "";
}

public class Circle : Shape
{
    public double Radius { get; set; }
}

public class Rectangle : Shape  
{
    public double Width { get; set; }
    public double Height { get; set; }
}