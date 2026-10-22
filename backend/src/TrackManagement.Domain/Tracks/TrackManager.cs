using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.Dsps;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Tracks;

/// <summary>
/// Entry point for the Track aggregate: application services use this instead of <see cref="ITrackRepository"/>.
/// Holds the rules that need repositories (artist exists, ISRC uniqueness, DSPs exist) and the reads services need.
/// </summary>
public sealed class TrackManager
{
    private readonly ITrackRepository _tracks;
    private readonly IArtistRepository _artists;
    private readonly IDspRepository _dsps;

    public TrackManager(ITrackRepository tracks, IArtistRepository artists, IDspRepository dsps)
    {
        _tracks = tracks;
        _artists = artists;
        _dsps = dsps;
    }

    /// <summary>The full aggregate, including distributions, for reading (not tracked).</summary>
    public async Task<Track> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _tracks.GetWithDistributionsAsync(id, cancellationToken)
        ?? throw new NotFoundException(nameof(Track), id);

    /// <summary>The full aggregate, tracked so changes made through its methods are saved.</summary>
    public async Task<Track> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _tracks.GetForUpdateAsync(id, cancellationToken)
        ?? throw new NotFoundException(nameof(Track), id);

    /// <summary>Tracks matching the filter, without distributions.</summary>
    public Task<IReadOnlyList<Track>> GetListAsync(TrackFilter filter, CancellationToken cancellationToken = default) =>
        _tracks.GetListAsync(filter, cancellationToken);

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

        var track = Track.Create(title, artistId, isrc, releaseDate, genre);
        await _tracks.AddAsync(track, cancellationToken);

        return track;
    }

    public async Task DistributeAsync(
        Track track, IReadOnlyCollection<Guid> dspIds, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(track);
        ArgumentNullException.ThrowIfNull(dspIds);

        var existing = await _dsps.GetByIdsAsync(dspIds, cancellationToken);
        var missing = dspIds.Except(existing.Select(d => d.Id)).ToList();
        if (missing.Count > 0)
            throw new NotFoundException($"DSP(s) not found: {string.Join(", ", missing)}.");

        track.DistributeTo(dspIds, utcNow);
    }
}
