# Contributing to Sod

Thank you for considering contributing to Sod! We welcome contributions from the community and are excited to see what you can bring to the project.

## How to Contribute

### Reporting Bugs

If you find a bug, please report it by opening an issue on our [GitHub Issues](https://github.com/plsft/Sod/issues) page. Please include:

- A clear description of the issue
- Steps to reproduce the behavior
- Expected behavior
- Actual behavior
- Code samples if applicable
- Your environment (OS, .NET version, etc.)

### Suggesting Enhancements

We're always looking for ways to make Sod better. If you have an idea for an enhancement:

1. Check the [Issues](https://github.com/plsft/Sod/issues) page to see if someone else has already suggested it
2. If not, open a new issue with the tag "enhancement"
3. Describe your enhancement in detail
4. Explain why this enhancement would be useful

### Pull Requests

1. Fork the repository
2. Create a new branch from `main` for your feature or fix
3. Make your changes
4. Add or update tests as needed
5. Ensure all tests pass by running `dotnet test`
6. Update documentation if needed
7. Submit a pull request

#### Pull Request Guidelines

- Keep your changes focused and atomic
- Write clear, concise commit messages
- Follow the existing code style and conventions
- Add tests for any new functionality
- Update the README.md if you're adding new features
- Ensure your code compiles without warnings

## Development Setup

1. Clone the repository
```bash
git clone https://github.com/plsft/Sod.git
cd Sod
```

2. Build the project
```bash
dotnet build
```

3. Run tests
```bash
dotnet test
```

## Code Style

- Use 4 spaces for indentation (not tabs)
- Follow C# naming conventions
- Keep lines under 120 characters when possible
- Use meaningful variable and method names
- Add XML documentation comments for public APIs

## Testing

- Write unit tests for all new functionality
- Ensure existing tests continue to pass
- Aim for high code coverage
- Use FluentAssertions for test assertions

## Documentation

- Update the README.md for any new features
- Add XML documentation comments to public methods and classes
- Include examples in your documentation

## Questions?

Feel free to open an issue with the "question" tag if you have any questions about contributing.

Thank you for contributing to Sod!