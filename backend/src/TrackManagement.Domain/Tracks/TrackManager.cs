using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Tracks;

/// <summary>Track rules that need repositories (artist exists, ISRC uniqueness).</summary>
public sealed class TrackManager(ITrackRepository tracks, IArtistRepository artists)
{
    public async Task<Track> CreateAsync(
        string title,
        Guid artistId,
        Isrc isrc,
        DateOnly releaseDate,
        string genre,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(isrc);

        if (!await artists.ExistsAsync(artistId, cancellationToken))
            throw new EntityNotFoundException(nameof(Artist), artistId);

        if (await tracks.IsrcExistsAsync(isrc, cancellationToken))
            throw new DuplicateEntityException(nameof(Track), nameof(Track.Isrc), isrc);

        return Track.Create(title, artistId, isrc, releaseDate, genre);
    }
}
