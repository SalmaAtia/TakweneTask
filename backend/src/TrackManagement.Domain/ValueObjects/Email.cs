using System.Text.RegularExpressions;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Shared;

namespace TrackManagement.Domain.ValueObjects;

/// <summary>Email address, normalized to lowercase so uniqueness is case-insensitive.</summary>
public sealed partial record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email is required.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > ValidationConstants.Email.MaxLength)
            throw new DomainException($"Email must be at most {ValidationConstants.Email.MaxLength} characters.");

        if (!Pattern().IsMatch(normalized))
            throw new DomainException($"'{value}' is not a valid email address.");

        return new Email(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(ValidationConstants.Email.Pattern)]
    private static partial Regex Pattern();
}
