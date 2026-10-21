using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Tracks;

public interface ITrackRepository
{
    /// <summary>Loads the full aggregate, including its distributions.</summary>
    Task<Track?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Track>> GetByArtistAsync(Guid artistId, CancellationToken cancellationToken = default);

    Task<bool> IsrcExistsAsync(Isrc isrc, CancellationToken cancellationToken = default);

    void Add(Track track);

    void Remove(Track track);
}
