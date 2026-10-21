namespace TrackManagement.Application.Tracks.Dtos;

/// <summary>Status is a string so invalid values reach the validator (names only, case-insensitive).</summary>
public sealed record ChangeTrackStatusDto(string Status);
