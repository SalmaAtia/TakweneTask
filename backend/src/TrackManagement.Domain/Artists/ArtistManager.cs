using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Artists;

/// <summary>Artist rules that need a repository (email uniqueness).</summary>
public sealed class ArtistManager(IArtistRepository artists)
{
    public async Task<Artist> CreateAsync(
        string name, Email email, CountryCode country, CancellationToken cancellationToken = default)
    {
        await EnsureEmailIsUniqueAsync(email, cancellationToken);

        return Artist.Create(name, email, country);
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
        if (await artists.EmailExistsAsync(email, cancellationToken))
            throw new DuplicateEntityException(nameof(Artist), nameof(Artist.Email), email);
    }
}
