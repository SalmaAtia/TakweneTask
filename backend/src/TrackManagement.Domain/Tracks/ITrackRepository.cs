using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Tracks;

public interface ITrackRepository
{
    /// <summary>Loads the full aggregate, including its distributions, for reading (not tracked).</summary>
    Task<Track?> GetWithDistributionsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Loads the full aggregate, including its distributions, tracked so changes are saved.</summary>
    Task<Track?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Tracks matching the filter, without distributions.</summary>
    Task<IReadOnlyList<Track>> GetListAsync(TrackFilter filter, CancellationToken cancellationToken = default);

    Task<bool> IsrcExistsAsync(Isrc isrc, CancellationToken cancellationToken = default);

    Task AddAsync(Track track, CancellationToken cancellationToken = default);

    void Remove(Track track);
}
