namespace TrackManagement.Application.Tracks.Dtos;

public sealed record CreateTrackDto(string Title, Guid ArtistId, string Isrc, DateOnly ReleaseDate, string Genre);
