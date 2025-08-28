using Sod;

// Example showing how unions work in Sod
class UnionExample
{
    static void Main()
    {
        Console.WriteLine("=== Sod Union Examples ===\n");

        // 1. Simple Union: Multiple string validations
        Console.WriteLine("1. Union with Different String Validations:");
        
        var emailOrPhone = Sod.Union(
            Sod.String().Email(),           // Must be valid email
            Sod.String().Regex(@"^\d{10}$") // Must be 10-digit phone
        );

        var emailTest = emailOrPhone.Parse("user@example.com");
        var phoneTest = emailOrPhone.Parse("1234567890");
        var invalidTest = emailOrPhone.Parse("not-valid");

        Console.WriteLine($"Email: Success={emailTest.Success}, Data='{emailTest.Data}'");
        Console.WriteLine($"Phone: Success={phoneTest.Success}, Data='{phoneTest.Data}'");  
        Console.WriteLine($"Invalid: Success={invalidTest.Success}");
        if (!invalidTest.Success)
        {
            Console.WriteLine($"  Error: {invalidTest.Error}");
        }

        Console.WriteLine();

        // 2. Discriminated Union (like TypeScript discriminated unions)
        Console.WriteLine("2. Discriminated Union with Shape Types:");
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

        // 3. Union for flexible validation
        Console.WriteLine("3. Union for Flexible Validation:");
        
        var numberOrStringNumber = Sod.Union(
            Sod.Number().Min(0).Max(100),
            Sod.String().Regex(@"^\d+$").Transform(s => int.Parse(s))
        );

        var directNumber = numberOrStringNumber.Parse("42");
        var stringNumber = numberOrStringNumber.Parse("75");
        var invalidNumber = numberOrStringNumber.Parse("abc");

        Console.WriteLine($"Direct number: Success={directNumber.Success}, Data={directNumber.Data}");
        Console.WriteLine($"String number: Success={stringNumber.Success}, Data={stringNumber.Data}");
        Console.WriteLine($"Invalid: Success={invalidNumber.Success}");

        Console.WriteLine();

        // 4. How unions work internally
        Console.WriteLine("4. How Unions Work in C#:");
        Console.WriteLine("- Unions try each schema in order");
        Console.WriteLine("- First successful match wins");
        Console.WriteLine("- If all fail, returns combined error");
        Console.WriteLine("- Type safety maintained through generics");
        Console.WriteLine("- Discriminated unions use a 'discriminator' field to choose the right schema");
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
