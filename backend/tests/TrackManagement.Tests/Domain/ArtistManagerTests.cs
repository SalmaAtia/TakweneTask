using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.ValueObjects;
using TrackManagement.Tests.Fakes;

namespace TrackManagement.Tests.Domain;

public sealed class ArtistManagerTests
{
    private readonly InMemoryArtistRepository _artists = new();
    private readonly ArtistManager _manager;

    public ArtistManagerTests()
    {
        _manager = new ArtistManager(_artists);
    }

    [Fact]
    public async Task GetAsync_returns_the_artist()
    {
        var artist = await _manager.CreateAsync("Nour El Sayed", Email.Create("nour@example.com"), CountryCode.Create("EG"));

        Assert.Same(artist, await _manager.GetAsync(artist.Id));
    }

    [Fact]
    public async Task GetAsync_throws_not_found_for_an_unknown_artist()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _manager.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_rejects_a_duplicate_email()
    {
        await _manager.CreateAsync("Nour El Sayed", Email.Create("nour@example.com"), CountryCode.Create("EG"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _manager.CreateAsync("Someone Else", Email.Create("NOUR@example.com"), CountryCode.Create("SA")));
        Assert.Single(_artists.Items);
    }
}
