using Microsoft.EntityFrameworkCore;
using TrackManagement.Domain.Artists;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class ArtistRepository : IArtistRepository
{
    private readonly AppDbContext _db;

    public ArtistRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Artists.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Artists.AsNoTracking().OrderBy(a => a.Name).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Artists.AnyAsync(a => a.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default) =>
        _db.Artists.AnyAsync(a => a.Email == email, cancellationToken);

    public async Task AddAsync(Artist artist, CancellationToken cancellationToken = default) =>
        await _db.Artists.AddAsync(artist, cancellationToken);
}
