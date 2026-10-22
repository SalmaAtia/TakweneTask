namespace TrackManagement.Application.Tracks.Dtos;

public sealed record TrackDetailDto(
    Guid Id,
    string Title,
    Guid ArtistId,
    string ArtistName,
    string Isrc,
    DateOnly ReleaseDate,
    string Genre,
    string Status,
    IReadOnlyList<TrackDistributionDto> Distributions);

public sealed record TrackDistributionDto(Guid DspId, string DspName, string Status, DateTimeOffset SubmittedAt);
