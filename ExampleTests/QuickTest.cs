using Sod;
using System;

class QuickTest
{
    static void Main()
    {
        Console.WriteLine("Testing basic Sod API access...");
        
        try 
        {
            // Test basic string schema
            var stringSchema = Sod.String();
            var result = stringSchema.Parse("test");
            Console.WriteLine($"String schema works: {result.Success}");
            
            // Test number schema
            var numberSchema = Sod.Number();
            var numResult = numberSchema.Parse("42");
            Console.WriteLine($"Number schema works: {numResult.Success}");
            
            Console.WriteLine("✅ Basic factory methods work!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}