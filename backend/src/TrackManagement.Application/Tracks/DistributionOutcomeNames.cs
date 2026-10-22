using TrackManagement.Domain.Tracks;
using TrackManagement.Shared.Enums;

namespace TrackManagement.Application.Tracks;

/// <summary>
/// The distribution statuses a caller may set: the outcomes of a Pending distribution.
/// Parsed by name only (numeric values like "1" are rejected).
/// </summary>
internal static class DistributionOutcomeNames
{
    private static readonly string[] Names = [nameof(DistributionStatus.Live), nameof(DistributionStatus.Rejected)];

    public static string ValidationMessage { get; } = $"'Status' must be one of: {string.Join(", ", Names)}.";

    public static bool IsValid(string? value) => Names.Contains(value, StringComparer.OrdinalIgnoreCase);

    public static DistributionStatus Parse(string value) => Enum.Parse<DistributionStatus>(value, ignoreCase: true);
}
