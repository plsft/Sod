using System.Text.RegularExpressions;
using Sod.Core;

namespace Sod.Schemas;

/// <summary>
/// String validation schema with various built-in validators
/// </summary>
public class SodString : SodSchema<string>
{
    private int? _minLength;
    private int? _maxLength;
    private int? _exactLength;
    private Regex? _regex;
    private string? _regexMessage;
    private readonly List<Func<string, bool>> _validators = new();
    private readonly List<string> _validatorMessages = new();
    private bool _trim;
    private bool _toLowerCase;
    private bool _toUpperCase;

    public SodString Min(int min)
    {
        _minLength = min;
        return this;
    }

    public SodString Max(int max)
    {
        _maxLength = max;
        return this;
    }

    public SodString Length(int length)
    {
        _exactLength = length;
        return this;
    }

    public SodString Email()
    {
        _validators.Add(value => System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"));
        _validatorMessages.Add("Invalid email format");
        return this;
    }

    public SodString Url()
    {
        _validators.Add(value => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
        _validatorMessages.Add("Invalid URL format");
        return this;
    }

    public SodString Uuid()
    {
        _validators.Add(value => Guid.TryParse(value, out _));
        _validatorMessages.Add("Invalid UUID format");
        return this;
    }

    public SodString Regex(Regex regex, string? message = null)
    {
        _regex = regex;
        _regexMessage = message ?? "String does not match pattern";
        return this;
    }

    public SodString Regex(string pattern, string? message = null)
    {
        return Regex(new Regex(pattern), message);
    }

    public SodString StartsWith(string prefix)
    {
        _validators.Add(value => value.StartsWith(prefix));
        _validatorMessages.Add($"String must start with '{prefix}'");
        return this;
    }

    public SodString EndsWith(string suffix)
    {
        _validators.Add(value => value.EndsWith(suffix));
        _validatorMessages.Add($"String must end with '{suffix}'");
        return this;
    }

    public SodString Contains(string substring)
    {
        _validators.Add(value => value.Contains(substring));
        _validatorMessages.Add($"String must contain '{substring}'");
        return this;
    }

    public SodString Cuid()
    {
        _validators.Add(value => System.Text.RegularExpressions.Regex.IsMatch(value, @"^c[a-z0-9]{24}$"));
        _validatorMessages.Add("Invalid CUID format");
        return this;
    }

    public SodString Cuid2()
    {
        _validators.Add(value => System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-z0-9]{24,}$"));
        _validatorMessages.Add("Invalid CUID2 format");
        return this;
    }

    public SodString Ulid()
    {
        _validators.Add(value => System.Text.RegularExpressions.Regex.IsMatch(value, @"^[0-9A-Z]{26}$"));
        _validatorMessages.Add("Invalid ULID format");
        return this;
    }

    public SodString Ip()
    {
        _validators.Add(value => System.Net.IPAddress.TryParse(value, out _));
        _validatorMessages.Add("Invalid IP address");
        return this;
    }

    public SodString Datetime()
    {
        _validators.Add(value => DateTime.TryParse(value, out _));
        _validatorMessages.Add("Invalid datetime format");
        return this;
    }

    public SodString NonEmpty()
    {
        _validators.Add(value => !string.IsNullOrWhiteSpace(value));
        _validatorMessages.Add("String cannot be empty");
        return this;
    }

    public SodString Trim()
    {
        _trim = true;
        return this;
    }

    public SodString ToLowerCase()
    {
        _toLowerCase = true;
        return this;
    }

    public SodString ToUpperCase()
    {
        _toUpperCase = true;
        return this;
    }

    public override SodResult<string> Parse(object? input)
    {
        input = ApplyPreprocess(input);

        // Handle null/undefined with optional/nullable/default
        if (input == null)
        {
            if (_hasDefault)
                return Ok(_defaultValue!);
            if (_isOptional || _isNullable)
                return Ok(null!);
            return Fail("Expected a string, got null");
        }

        if (input is not string str)
            return Fail($"Expected a string, got {input.GetType().Name}");

        // Apply transformations
        if (_trim) str = str.Trim();
        if (_toLowerCase) str = str.ToLowerInvariant();
        if (_toUpperCase) str = str.ToUpperInvariant();

        // Length validations
        if (_exactLength.HasValue && str.Length != _exactLength.Value)
            return Fail($"String must be exactly {_exactLength.Value} characters");

        if (_minLength.HasValue && str.Length < _minLength.Value)
            return Fail($"String must be at least {_minLength.Value} characters");

        if (_maxLength.HasValue && str.Length > _maxLength.Value)
            return Fail($"String must be at most {_maxLength.Value} characters");

        // Regex validation
        if (_regex != null && !_regex.IsMatch(str))
            return Fail(_regexMessage!);

        // Custom validators
        for (int i = 0; i < _validators.Count; i++)
        {
            if (!_validators[i](str))
                return Fail(_validatorMessages[i]);
        }

        // Apply refinements
        return ApplyRefinements(str);
    }
}