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

    public Task<Track?> GetWithDistributionsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Tracks
            .AsNoTracking()
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Track?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Track>> GetListAsync(TrackFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _db.Tracks.AsNoTracking();

        if (filter.ArtistId is { } artistId)
            query = query.Where(t => t.ArtistId == artistId);

        if (!string.IsNullOrWhiteSpace(filter.Genre))
        {
            // Explicitly case-insensitive, independent of the database collation.
            var genre = filter.Genre.Trim().ToLowerInvariant();
            query = query.Where(t => t.Genre.ToLower() == genre);
        }

        if (filter.Status is { } status)
            query = query.Where(t => t.Status == status);

        return await query
            .OrderByDescending(t => t.ReleaseDate)
            .ThenBy(t => t.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsrcExistsAsync(Isrc isrc, CancellationToken cancellationToken = default) =>
        _db.Tracks.AnyAsync(t => t.Isrc == isrc, cancellationToken);

    public async Task AddAsync(Track track, CancellationToken cancellationToken = default) =>
        await _db.Tracks.AddAsync(track, cancellationToken);

    public void Remove(Track track) => _db.Tracks.Remove(track);
}
