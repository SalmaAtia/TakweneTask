using System.Text.RegularExpressions;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Shared;

namespace TrackManagement.Domain.ValueObjects;

/// <summary>ISO 3166-1 alpha-2 country code (format check only), e.g. "EG".</summary>
public sealed partial record CountryCode
{
    public string Value { get; }

    private CountryCode(string value) => Value = value;

    public static CountryCode Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Country code is required.");

        var normalized = value.Trim().ToUpperInvariant();

        if (!Pattern().IsMatch(normalized))
            throw new DomainException($"'{value}' is not a valid ISO 3166-1 alpha-2 country code.");

        return new CountryCode(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(ValidationConstants.CountryCode.Pattern)]
    private static partial Regex Pattern();
}
