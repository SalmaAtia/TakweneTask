using Microsoft.EntityFrameworkCore;
using TrackManagement.Domain.Tracks;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class TrackRepository(AppDbContext db) : ITrackRepository
{
    public Task<Track?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Track>> GetByArtistAsync(Guid artistId, CancellationToken cancellationToken = default) =>
        await db.Tracks
            .AsNoTracking()
            .Include(t => t.Distributions)
            .Where(t => t.ArtistId == artistId)
            .OrderByDescending(t => t.ReleaseDate)
            .ToListAsync(cancellationToken);

    public Task<bool> IsrcExistsAsync(Isrc isrc, CancellationToken cancellationToken = default) =>
        db.Tracks.AnyAsync(t => t.Isrc == isrc, cancellationToken);

    public void Add(Track track) => db.Tracks.Add(track);

    public void Remove(Track track) => db.Tracks.Remove(track);
}
