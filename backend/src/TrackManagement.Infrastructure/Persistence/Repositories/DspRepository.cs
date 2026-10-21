using Microsoft.EntityFrameworkCore;
using TrackManagement.Domain.Dsps;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class DspRepository : IDspRepository
{
    private readonly AppDbContext _db;

    public DspRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Dsp?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Dsps.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Dsp>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Dsps.AsNoTracking().OrderBy(d => d.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Dsp>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await _db.Dsps
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Dsps.AnyAsync(d => d.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var trimmed = name.Trim();
        return _db.Dsps.AnyAsync(d => d.Name == trimmed, cancellationToken);
    }

    public async Task AddAsync(Dsp dsp, CancellationToken cancellationToken = default) =>
        await _db.Dsps.AddAsync(dsp, cancellationToken);
}
