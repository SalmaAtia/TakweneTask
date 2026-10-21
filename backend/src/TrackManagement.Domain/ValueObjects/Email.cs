using System.Text.RegularExpressions;
using TrackManagement.Domain.Common.Exceptions;

namespace TrackManagement.Domain.ValueObjects;

/// <summary>Email address, normalized to lowercase so uniqueness is case-insensitive.</summary>
public sealed partial record Email
{
    public const int MaxLength = 254;

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleViolationException("Email is required.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new BusinessRuleViolationException($"Email must be at most {MaxLength} characters.");

        if (!Pattern().IsMatch(normalized))
            throw new BusinessRuleViolationException($"'{value}' is not a valid email address.");

        return new Email(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex Pattern();
}
