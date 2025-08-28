using Sod.Core;
using Sod.Schemas;

namespace Sod;

/// <summary>
/// Main entry point for Sod schema validation
/// Provides static factory methods for all schema types
/// </summary>
public static class Sod
{
    // Primitive schemas
    public static SodString String() => new();
    public static SodNumber Number() => new();
    public static SodNumber Int() => new();
    public static SodFloat Float() => new();
    public static SodDecimal Decimal() => new();
    public static SodBoolean Boolean() => new();
    public static SodBoolean Bool() => Boolean();
    public static SodDate Date() => new();
    public static SodDate DateTime() => Date();
    public static SodDateOnly DateOnly() => new();
    public static SodTime Time() => new();
    public static SodTimeOnly TimeOnly() => new();
    
    // Literals
    public static SodLiteral<T> Literal<T>(T value) => new(value);
    
    // Enums
    public static SodEnum<T> Enum<T>() where T : struct, Enum => new();
    public static SodEnum<T> Enum<T>(params T[] values) where T : struct, Enum => new(values);
    public static SodNativeEnum NativeEnum(params string[] values) => new(values);
    
    // Collections
    public static SodArray<T> Array<T>(SodSchema<T> elementSchema) => new(elementSchema);
    public static SodSet<T> Set<T>(SodSchema<T> elementSchema) => new(elementSchema);
    public static SodTuple<T1, T2> Tuple<T1, T2>(SodSchema<T1> item1, SodSchema<T2> item2) => new(item1, item2);
    public static SodTuple<T1, T2, T3> Tuple<T1, T2, T3>(SodSchema<T1> item1, SodSchema<T2> item2, SodSchema<T3> item3) => new(item1, item2, item3);
    
    // Objects and records
    public static SodObject<T> Object<T>() where T : new() => new();
    public static SodObject<T> Object<T>(Action<SodObject<T>> shape) where T : new()
    {
        var schema = new SodObject<T>();
        shape(schema);
        return schema;
    }
    public static SodRecord<T> Record<T>(SodSchema<T> valueSchema) => new(valueSchema);
    public static SodRecord<T> Record<T>(SodSchema<T> valueSchema, SodSchema<string> keySchema) => new(valueSchema, keySchema);
    public static SodMap<TKey, TValue> Map<TKey, TValue>(SodSchema<TKey> keySchema, SodSchema<TValue> valueSchema) where TKey : notnull => new(keySchema, valueSchema);
    
    // Unions and intersections
    public static SodUnion<T> Union<T>(params SodSchema<T>[] schemas) => new(schemas);
    public static SodDiscriminatedUnion<T> DiscriminatedUnion<T>(string discriminator) => new(discriminator);
    public static SodIntersection<T> Intersection<T>(params SodSchema<T>[] schemas) => new(schemas);
    
    // Special schemas
    public static SodAny Any() => new();
    public static SodUnknown Unknown() => new();
    public static SodNever Never(string? message = null) => message != null ? new(message) : new();
    public static SodVoid Void() => new();
    public static SodNull Null() => new();
    public static SodUndefined Undefined() => new();
    
    // Transform and utility schemas
    public static SodTransform<TInput, TOutput> Transform<TInput, TOutput>(SodSchema<TInput> input, Func<TInput, TOutput> transform) => new(input, transform);
    public static SodLazy<T> Lazy<T>(Func<SodSchema<T>> factory) => new(factory);
    public static SodPipeline<T> Pipeline<T>(params SodSchema<T>[] schemas) => new(schemas);
    public static SodBrand<T, TBrand> Brand<T, TBrand>(SodSchema<T> schema, string brandName) => new(schema, brandName);
    public static SodCatch<T> Catch<T>(SodSchema<T> schema, T fallback) => new(schema, fallback);
    
    // Coercion helpers
    public static class Coerce
    {
        public static SodString String() => SodCoerce.String();
        public static SodNumber Number() => SodCoerce.Number();
        public static SodBoolean Boolean() => SodCoerce.Boolean();
        public static SodDate Date() => SodCoerce.Date();
        public static SodDecimal Decimal() => SodCoerce.Decimal();
        public static SodFloat Float() => SodCoerce.Float();
    }
    
    // Utility methods
    
    /// <summary>
    /// Creates an optional version of a schema
    /// </summary>
    public static SodSchema<T?> Optional<T>(SodSchema<T> schema)
    {
        schema.Optional();
        return schema as SodSchema<T?> ?? throw new InvalidOperationException();
    }
    
    /// <summary>
    /// Creates a nullable version of a schema
    /// </summary>
    public static SodSchema<T?> Nullable<T>(SodSchema<T> schema)
    {
        schema.Nullable();
        return schema as SodSchema<T?> ?? throw new InvalidOperationException();
    }
    
    /// <summary>
    /// Preprocesses input before validation
    /// </summary>
    public static SodSchema<T> Preprocess<T>(SodSchema<T> schema, Func<object, object> preprocessor)
    {
        schema.Preprocess(preprocessor);
        return schema;
    }
}