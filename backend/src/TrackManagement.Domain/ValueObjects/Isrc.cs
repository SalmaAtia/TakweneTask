using System.Text.RegularExpressions;
using TrackManagement.Domain.Common.Exceptions;

namespace TrackManagement.Domain.ValueObjects;

/// <summary>
/// International Standard Recording Code in canonical form CC-XXX-YY-NNNNN, e.g. "US-RC1-76-07839".
/// Input is accepted with or without hyphens/spaces and normalized to uppercase.
/// </summary>
public sealed partial record Isrc
{
    public const int Length = 15;

    public string Value { get; }

    private Isrc(string value) => Value = value;

    public static Isrc Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("ISRC is required.");

        var compact = value.Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .ToUpperInvariant();

        if (!CompactPattern().IsMatch(compact))
            throw new DomainException($"'{value}' is not a valid ISRC (expected CC-XXX-YY-NNNNN).");

        return new Isrc($"{compact[..2]}-{compact[2..5]}-{compact[5..7]}-{compact[7..]}");
    }

    public override string ToString() => Value;

    // CC (country) + XXX (registrant) + YY (year) + NNNNN (designation)
    [GeneratedRegex("^[A-Z]{2}[A-Z0-9]{3}[0-9]{2}[0-9]{5}$")]
    private static partial Regex CompactPattern();
}
