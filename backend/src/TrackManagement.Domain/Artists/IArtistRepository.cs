using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Artists;

public interface IArtistRepository
{
    Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);

    void Add(Artist artist);
}
