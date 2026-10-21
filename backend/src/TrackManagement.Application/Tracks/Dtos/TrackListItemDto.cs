namespace TrackManagement.Application.Tracks.Dtos;

public sealed record TrackListItemDto(
    Guid Id,
    string Title,
    Guid ArtistId,
    string ArtistName,
    string Isrc,
    DateOnly ReleaseDate,
    string Genre,
    string Status);
