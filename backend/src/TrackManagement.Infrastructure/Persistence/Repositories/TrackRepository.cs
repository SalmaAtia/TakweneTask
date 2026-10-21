using Microsoft.EntityFrameworkCore;
using TrackManagement.Domain.Tracks;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class TrackRepository : ITrackRepository
{
    private readonly AppDbContext _db;

    public TrackRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Track?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Track>> GetByArtistAsync(Guid artistId, CancellationToken cancellationToken = default) =>
        await _db.Tracks
            .AsNoTracking()
            .Include(t => t.Distributions)
            .Where(t => t.ArtistId == artistId)
            .OrderByDescending(t => t.ReleaseDate)
            .ToListAsync(cancellationToken);

    public Task<bool> IsrcExistsAsync(Isrc isrc, CancellationToken cancellationToken = default) =>
        _db.Tracks.AnyAsync(t => t.Isrc == isrc, cancellationToken);

    public async Task AddAsync(Track track, CancellationToken cancellationToken = default) =>
        await _db.Tracks.AddAsync(track, cancellationToken);

    public void Remove(Track track) => _db.Tracks.Remove(track);
}
