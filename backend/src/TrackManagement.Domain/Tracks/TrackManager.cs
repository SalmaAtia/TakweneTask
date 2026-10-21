using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Tracks;

/// <summary>Track rules that need repositories (artist exists, ISRC uniqueness).</summary>
public sealed class TrackManager
{
    private readonly ITrackRepository _tracks;
    private readonly IArtistRepository _artists;

    public TrackManager(ITrackRepository tracks, IArtistRepository artists)
    {
        _tracks = tracks;
        _artists = artists;
    }

    public async Task<Track> CreateAsync(
        string title,
        Guid artistId,
        Isrc isrc,
        DateOnly releaseDate,
        string genre,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(isrc);

        if (!await _artists.ExistsAsync(artistId, cancellationToken))
            throw new NotFoundException(nameof(Artist), artistId);

        if (await _tracks.IsrcExistsAsync(isrc, cancellationToken))
            throw new ConflictException($"A track with ISRC '{isrc}' already exists.");

        return Track.Create(title, artistId, isrc, releaseDate, genre);
    }
}
