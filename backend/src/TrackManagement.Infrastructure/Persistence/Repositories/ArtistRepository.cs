using Microsoft.EntityFrameworkCore;
using TrackManagement.Domain.Artists;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class ArtistRepository(AppDbContext db) : IArtistRepository
{
    public Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Artists.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Artists.AsNoTracking().OrderBy(a => a.Name).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Artists.AnyAsync(a => a.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default) =>
        db.Artists.AnyAsync(a => a.Email == email, cancellationToken);

    public void Add(Artist artist) => db.Artists.Add(artist);
}
