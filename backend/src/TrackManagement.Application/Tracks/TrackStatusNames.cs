using TrackManagement.Domain.Tracks;

namespace TrackManagement.Application.Tracks;

/// <summary>Parses TrackStatus from API strings by name only (numeric values like "1" are rejected).</summary>
internal static class TrackStatusNames
{
    private static readonly string[] Names = Enum.GetNames<TrackStatus>();

    public static string ValidationMessage { get; } = $"'Status' must be one of: {string.Join(", ", Names)}.";

    public static bool IsValid(string? value) => Names.Contains(value, StringComparer.OrdinalIgnoreCase);

    public static TrackStatus Parse(string value) => Enum.Parse<TrackStatus>(value, ignoreCase: true);
}
