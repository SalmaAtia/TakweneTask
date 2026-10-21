namespace TrackManagement.Domain.Dsps;

public interface IDspRepository
{
    Task<Dsp?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dsp>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dsp>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(Dsp dsp, CancellationToken cancellationToken = default);
}
