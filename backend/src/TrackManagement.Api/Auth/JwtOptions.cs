using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TrackManagement.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    // HMAC-SHA256 needs a key of at least 256 bits.
    public const int MinSigningKeyBytes = 32;

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; }

    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(SigningKey));
}

internal sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
            failures.Add($"{JwtOptions.SectionName}:Issuer is required.");
        if (string.IsNullOrWhiteSpace(options.Audience))
            failures.Add($"{JwtOptions.SectionName}:Audience is required.");
        if (string.IsNullOrWhiteSpace(options.SigningKey))
            failures.Add($"{JwtOptions.SectionName}:SigningKey is required.");
        else if (Encoding.UTF8.GetByteCount(options.SigningKey) < JwtOptions.MinSigningKeyBytes)
            failures.Add($"{JwtOptions.SectionName}:SigningKey must be at least {JwtOptions.MinSigningKeyBytes} bytes.");
        if (options.ExpiryMinutes <= 0)
            failures.Add($"{JwtOptions.SectionName}:ExpiryMinutes must be greater than 0.");

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
