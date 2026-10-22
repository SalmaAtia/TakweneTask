using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Dsps;
using TrackManagement.Domain.Tracks;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Tests.Fakes;

// Minimal in-memory fakes of the repository interfaces for domain manager tests.
// Only the members the managers use do real work; the rest aren't needed and throw.

internal sealed class InMemoryTrackRepository : ITrackRepository
{
    public List<Track> Items { get; } = [];

    public Task<bool> IsrcExistsAsync(Isrc isrc, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(t => t.Isrc == isrc));

    public Task AddAsync(Track track, CancellationToken cancellationToken = default)
    {
        Items.Add(track);
        return Task.CompletedTask;
    }

    public Task<Track?> GetWithDistributionsAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Track?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<IReadOnlyList<Track>> GetListAsync(TrackFilter filter, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public void Remove(Track track) => throw new NotSupportedException();
}

internal sealed class InMemoryArtistRepository : IArtistRepository
{
    public List<Artist> Items { get; } = [];

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(a => a.Id == id));

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(a => a.Email == email));

    public Task AddAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        Items.Add(artist);
        return Task.CompletedTask;
    }

    public Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<IReadOnlyList<Artist>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

internal sealed class InMemoryDspRepository : IDspRepository
{
    public List<Dsp> Items { get; } = [];

    public Task<IReadOnlyList<Dsp>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Dsp>>(Items.Where(d => ids.Contains(d.Id)).ToList());

    public Task<Dsp?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<IReadOnlyList<Dsp>> GetAllAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task AddAsync(Dsp dsp, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}
