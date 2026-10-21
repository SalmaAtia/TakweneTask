using TrackManagement.Application.Tracks.Dtos;
using TrackManagement.Domain.Tracks;

namespace TrackManagement.Application.Tracks;

internal static class TrackMappings
{
    public static TrackListItemDto ToListItemDto(this Track track, string artistName) =>
        new(
            track.Id,
            track.Title,
            track.ArtistId,
            artistName,
            track.Isrc.Value,
            track.ReleaseDate,
            track.Genre,
            track.Status.ToString());

    public static TrackDetailDto ToDetailDto(this Track track, IReadOnlyDictionary<Guid, string> dspNames) =>
        new(
            track.Id,
            track.Title,
            track.ArtistId,
            track.Isrc.Value,
            track.ReleaseDate,
            track.Genre,
            track.Status.ToString(),
            track.Distributions
                .Select(d => new TrackDistributionDto(d.DspId, dspNames[d.DspId], d.Status.ToString(), d.SubmittedAt))
                .OrderBy(d => d.SubmittedAt)
                .ThenBy(d => d.DspName)
                .ToList());
}
