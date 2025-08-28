# Sod - Schema Validation for .NET

[![NuGet](https://img.shields.io/nuget/v/Sod.svg)](https://www.nuget.org/packages/Sod/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A powerful, Zod-inspired fluent validation schema library for .NET. Sod provides type-safe schema definitions with a clean, intuitive API inspired by [Zod](https://github.com/colinhacks/zod) for TypeScript.

## Features

- **Fluent and readable API** - Chain validation methods for clean, expressive schemas
- **Strongly-typed validation** - Full IntelliSense support and compile-time type safety
- **Comprehensive validators** - String, number, date, array, object, unions, and more
- **Advanced features** - Transforms, refinements, coercion, preprocessing
- **Clear error messages** - Detailed validation errors with path information
- **Zero dependencies** - Lightweight, standalone library
- **.NET 8+ support** - Modern C# features and nullable reference types

## Installation

Install via NuGet:

```bash
dotnet add package Sod
```

Or via Package Manager Console:

```powershell
Install-Package Sod
```

## Quick Start

```csharp
using Sod;

// Define a schema
var userSchema = Sod.Object<User>()
    .Field(u => u.Username, Sod.String().Min(3).Max(20))
    .Field(u => u.Email, Sod.String().Email())
    .Field(u => u.Age, Sod.Number().Min(18).Max(100))
    .Field(u => u.Website, Sod.String().Url().Optional());

// Validate data
var result = userSchema.Parse(new Dictionary<string, object>
{
    { "Username", "johndoe" },
    { "Email", "john@example.com" },
    { "Age", 25 }
});

if (result.Success)
{
    var user = result.Data;
    Console.WriteLine($"Valid user: {user.Username}");
}
else
{
    Console.WriteLine($"Validation errors: {result.Error}");
}
```

## Schema Types

### Primitives

```csharp
Sod.String()      // string
Sod.Number()      // int
Sod.Float()       // float
Sod.Decimal()     // decimal
Sod.Boolean()     // bool
Sod.Date()        // DateTime
Sod.DateOnly()    // DateOnly
Sod.TimeOnly()    // TimeOnly
```

### String Validations

```csharp
Sod.String()
    .Min(5)                    // Minimum length
    .Max(100)                  // Maximum length
    .Length(10)                // Exact length
    .Email()                   // Valid email
    .Url()                     // Valid URL
    .Uuid()                    // Valid UUID/GUID
    .Regex(@"^\d+$")          // Custom regex
    .StartsWith("https://")    // Starts with
    .EndsWith(".com")          // Ends with
    .Contains("@")             // Contains substring
    .NonEmpty()                // Not empty or whitespace
    .Trim()                    // Trim whitespace
    .ToUpperCase()             // Convert to uppercase
    .ToLowerCase()             // Convert to lowercase
```

### Number Validations

```csharp
Sod.Number()
    .Min(0)                    // Minimum value
    .Max(100)                  // Maximum value
    .Positive()                // > 0
    .Negative()                // < 0
    .NonNegative()             // >= 0
    .NonPositive()             // <= 0
    .MultipleOf(5)             // Divisible by
    .Int()                     // Integer (default for Number)
    .Finite()                  // Not infinity
```

### Arrays and Collections

```csharp
// Arrays
Sod.Array(Sod.String())
    .Min(1)                    // Minimum items
    .Max(10)                   // Maximum items
    .Length(5)                 // Exact length
    .NonEmpty()                // At least one item

// Sets (unique values)
Sod.Set(Sod.Number())
    .Min(3)                    // Minimum unique items

// Tuples
Sod.Tuple(Sod.String(), Sod.Number())  // (string, int)
```

### Objects

```csharp
// Define object schemas
var personSchema = Sod.Object<Person>()
    .Field(p => p.Name, Sod.String())
    .Field(p => p.Age, Sod.Number())
    .Field(p => p.Email, Sod.String().Email().Optional());

// Object modifiers
schema.Strict()        // No unknown keys allowed
schema.Strip()         // Remove unknown keys (default)
schema.Passthrough()   // Allow unknown keys
schema.Partial()       // Make all fields optional
schema.Required()      // Make all fields required
schema.Pick("Name", "Age")    // Include only specified fields
schema.Omit("Email")          // Exclude specified fields
```

### Enums and Literals

```csharp
// C# Enums
public enum Status { Active, Inactive, Pending }
var statusSchema = Sod.Enum<Status>();

// String enums
var roleSchema = Sod.NativeEnum("admin", "user", "guest");

// Literal values
var trueSchema = Sod.Literal(true);
var constantSchema = Sod.Literal("CONSTANT_VALUE");
```

### Unions and Intersections

```csharp
// Union (OR) - value must match one of the schemas
var stringOrNumber = Sod.Union(
    Sod.String(),
    Sod.Number()
);

// Discriminated union
var shapeSchema = Sod.DiscriminatedUnion<Shape>("type")
    .Option("circle", circleSchema)
    .Option("rectangle", rectangleSchema);

// Intersection (AND) - value must match all schemas
var namedPerson = Sod.Intersection(
    personSchema,
    hasNameSchema
);
```

### Optional and Nullable

```csharp
// Optional - can be undefined/missing
Sod.String().Optional()

// Nullable - can be null
Sod.String().Nullable()

// Default value
Sod.String().Default("default value")
```

### Transforms

```csharp
// Transform parsed value
var upperString = Sod.String()
    .Transform(s => s.ToUpper());

// Parse string to number, then double it
var doubledNumber = Sod.String()
    .Transform(s => int.Parse(s))
    .Transform(n => n * 2);

// Preprocess input before validation
var trimmedString = Sod.String()
    .Preprocess(input => input?.ToString()?.Trim())
    .Min(3);
```

### Refinements

```csharp
// Custom validation logic
var passwordSchema = Sod.String()
    .Min(8)
    .Refine(
        pwd => pwd.Any(char.IsDigit),
        "Password must contain at least one digit"
    )
    .Refine(
        pwd => pwd.Any(char.IsUpper),
        "Password must contain at least one uppercase letter"
    );
```

### Coercion

```csharp
// Coerce values to target type
Sod.Coerce.String()    // Convert to string
Sod.Coerce.Number()    // Convert to number
Sod.Coerce.Boolean()   // Convert to boolean
Sod.Coerce.Date()      // Convert to DateTime

// Examples
Sod.Coerce.Number().Parse("42")     // Returns 42
Sod.Coerce.Boolean().Parse("true")  // Returns true
Sod.Coerce.Boolean().Parse(1)       // Returns true
```

### Advanced Features

```csharp
// Lazy evaluation for recursive types
var categorySchema = Sod.Lazy(() => 
    Sod.Object<Category>()
        .Field(c => c.Name, Sod.String())
        .Field(c => c.Subcategories, Sod.Array(categorySchema).Optional())
);

// Pipeline - chain multiple validations
var processedString = Sod.Pipeline(
    Sod.String(),
    Sod.String().Min(3),
    Sod.String().Transform(s => s.ToUpper())
);

// Catch - provide fallback on error
var safeNumber = Sod.Catch(Sod.Number(), 0);

// Branded types for nominal typing
var emailSchema = Sod.Brand<string, Email>(
    Sod.String().Email(),
    "Email"
);

// Record/Dictionary validation
var configSchema = Sod.Record(Sod.String());
var typedMap = Sod.Map(Sod.String(), Sod.Number());
```

## Error Handling

Sod provides multiple ways to handle validation errors:

```csharp
// Safe parsing (returns result object)
var result = schema.SafeParse(input);
if (result.Success)
{
    var data = result.Data;
}
else
{
    var errors = result.Errors; // Array of error messages
}

// Parse or throw exception
try
{
    var data = schema.ParseOrThrow(input);
}
catch (SodValidationException ex)
{
    Console.WriteLine(ex.Message);
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

## Complete Example

```csharp
using Sod;
using System;
using System.Collections.Generic;

// Define your models
public class User
{
    public string Username { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public UserRole Role { get; set; }
    public List<string> Tags { get; set; }
    public Address Address { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
}

public enum UserRole
{
    Admin,
    User,
    Guest
}

// Create schemas
var addressSchema = Sod.Object<Address>()
    .Field(a => a.Street, Sod.String().NonEmpty())
    .Field(a => a.City, Sod.String().NonEmpty())
    .Field(a => a.PostalCode, Sod.String().Regex(@"^\d{5}$"));

var userSchema = Sod.Object<User>()
    .Field(u => u.Username, 
        Sod.String()
            .Min(3)
            .Max(20)
            .Regex(@"^[a-zA-Z0-9_]+$"))
    .Field(u => u.Email, 
        Sod.String().Email())
    .Field(u => u.Age, 
        Sod.Number()
            .Min(13)
            .Max(120))
    .Field(u => u.Role, 
        Sod.Enum<UserRole>())
    .Field(u => u.Tags, 
        Sod.Array(Sod.String())
            .Max(10)
            .Default(new List<string>()))
    .Field(u => u.Address, addressSchema)
    .Field(u => u.CreatedAt, 
        Sod.Date()
            .Default(DateTime.Now));

// Validate input
var input = new Dictionary<string, object>
{
    { "Username", "john_doe" },
    { "Email", "john@example.com" },
    { "Age", 25 },
    { "Role", "User" },
    { "Tags", new[] { "developer", "gamer" } },
    { "Address", new Dictionary<string, object>
        {
            { "Street", "123 Main St" },
            { "City", "New York" },
            { "PostalCode", "10001" }
        }
    }
};

var result = userSchema.Parse(input);

if (result.Success)
{
    var user = result.Data;
    Console.WriteLine($"User created: {user.Username} ({user.Email})");
    Console.WriteLine($"Role: {user.Role}, Age: {user.Age}");
    Console.WriteLine($"Address: {user.Address.Street}, {user.Address.City}");
}
```

## Performance Considerations

- Schemas are immutable - methods return new schema instances
- Reuse schema instances when possible
- Consider using `Lazy` for recursive schemas
- Preprocessing runs before all validations
- Transforms run after successful validation

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by [Zod](https://github.com/colinhacks/zod) for TypeScript
- Built with modern C# and .NET features

## Support

- Report issues on [GitHub Issues](https://github.com/plsft/Sod/issues)
- Star the project if you find it useful!