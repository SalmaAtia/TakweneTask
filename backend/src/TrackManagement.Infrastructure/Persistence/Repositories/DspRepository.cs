using Microsoft.EntityFrameworkCore;
using TrackManagement.Domain.Dsps;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class DspRepository(AppDbContext db) : IDspRepository
{
    public Task<Dsp?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Dsps.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Dsp>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Dsps.AsNoTracking().OrderBy(d => d.Name).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Dsps.AnyAsync(d => d.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var trimmed = name.Trim();
        return db.Dsps.AnyAsync(d => d.Name == trimmed, cancellationToken);
    }

    public void Add(Dsp dsp) => db.Dsps.Add(dsp);
}
