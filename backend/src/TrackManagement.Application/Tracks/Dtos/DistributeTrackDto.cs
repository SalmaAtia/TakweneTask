namespace TrackManagement.Application.Tracks.Dtos;

public sealed record DistributeTrackDto(IReadOnlyList<Guid> DspIds);
