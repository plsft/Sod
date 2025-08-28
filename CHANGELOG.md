# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2024-08-27

### Added

#### Core Features
- Complete rewrite with Zod v4 feature parity
- Strongly-typed schema definitions with IntelliSense support
- Comprehensive validation error messages with path information
- Zero external dependencies

#### Schema Types
- **Primitives**: String, Number, Float, Decimal, Boolean, Date, DateOnly, TimeOnly
- **Collections**: Array, Set, Tuple, Record, Map
- **Objects**: Object schemas with field validation, strict/strip/passthrough modes
- **Enums**: Native enums, string enums, literal values
- **Unions**: Union types, discriminated unions, intersections
- **Special**: Any, Unknown, Never, Void, Null, Undefined

#### Validation Features
- **String validators**: Email, URL, UUID, CUID, ULID, IP, Regex, StartsWith, EndsWith, Contains
- **Number validators**: Min, Max, Positive, Negative, MultipleOf, Finite
- **Array validators**: Min, Max, Length, NonEmpty
- **Date validators**: Min, Max date ranges

#### Advanced Features
- **Transforms**: Transform parsed values with type safety
- **Refinements**: Custom validation logic with error messages
- **Coercion**: Automatic type coercion for strings, numbers, booleans, dates
- **Preprocessing**: Modify input before validation
- **Default values**: Provide defaults for missing/null values
- **Optional/Nullable**: Flexible handling of undefined and null values
- **Lazy evaluation**: Support for recursive schemas
- **Pipeline**: Chain multiple validations
- **Branded types**: Nominal typing support
- **Catch**: Fallback values on validation errors

#### Developer Experience
- Fluent API for intuitive schema building
- Method chaining for readable validation rules
- Clear error messages with detailed validation failures
- Full nullable reference type support
- .NET 8+ optimizations

### Changed
- Complete API redesign following Zod patterns
- Improved performance with immutable schemas
- Better error reporting with structured error arrays
- Enhanced IntelliSense documentation

### Fixed
- Type safety issues from v1
- Validation edge cases
- Null handling inconsistencies

## [1.0.0] - 2024-01-01

### Added
- Initial release
- Basic string, number, date validation
- Simple object validation
- Array validation
- Source-only NuGet package support