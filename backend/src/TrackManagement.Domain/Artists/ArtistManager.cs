using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Artists;

/// <summary>
/// Entry point for the Artist aggregate: application services use this instead of <see cref="IArtistRepository"/>.
/// Holds the rules that need a repository (email uniqueness) and the reads services need.
/// </summary>
public sealed class ArtistManager
{
    private readonly IArtistRepository _artists;

    public ArtistManager(IArtistRepository artists)
    {
        _artists = artists;
    }

    public async Task<Artist> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _artists.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(nameof(Artist), id);

    public Task<IReadOnlyList<Artist>> GetListAsync(CancellationToken cancellationToken = default) =>
        _artists.GetAllAsync(cancellationToken);

    /// <summary>One batched lookup; unknown ids are simply absent from the result.</summary>
    public Task<IReadOnlyList<Artist>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        _artists.GetByIdsAsync(ids, cancellationToken);

    public async Task<Artist> CreateAsync(
        string name, Email email, CountryCode country, CancellationToken cancellationToken = default)
    {
        await EnsureEmailIsUniqueAsync(email, cancellationToken);

        var artist = Artist.Create(name, email, country);
        await _artists.AddAsync(artist, cancellationToken);

        return artist;
    }

    public async Task ChangeEmailAsync(Artist artist, Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(artist);

        if (artist.Email == email)
            return;

        await EnsureEmailIsUniqueAsync(email, cancellationToken);

        artist.ChangeEmail(email);
    }

    private async Task EnsureEmailIsUniqueAsync(Email email, CancellationToken cancellationToken)
    {
        if (await _artists.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException($"An artist with email '{email}' already exists.");
    }
}
