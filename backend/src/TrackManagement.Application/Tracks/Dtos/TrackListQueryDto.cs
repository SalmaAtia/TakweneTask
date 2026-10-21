namespace TrackManagement.Application.Tracks.Dtos;

/// <summary>Query-string filters for the track list. Status is a string so invalid values reach the validator.</summary>
public sealed record TrackListQueryDto(Guid? ArtistId, string? Genre, string? Status);
