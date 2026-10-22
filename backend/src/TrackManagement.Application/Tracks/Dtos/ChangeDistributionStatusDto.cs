namespace TrackManagement.Application.Tracks.Dtos;

/// <summary>The DSP's answer for a Pending distribution: "Live" or "Rejected" (names only, case-insensitive).</summary>
public sealed record ChangeDistributionStatusDto(string Status);
