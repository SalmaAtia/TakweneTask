using System.Net;
using System.Net.Http.Json;
using TrackManagement.Application.Artists.Dtos;
using TrackManagement.Application.Dsps.Dtos;
using TrackManagement.Application.Tracks.Dtos;

namespace TrackManagement.Tests.Integration;

/// <summary>
/// End-to-end walk through every artist/track endpoint, so each application service method
/// (and the manager reads behind it) runs against the real database at least once.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class CatalogFlowApiTests
{
    // From the seed data (Infrastructure/Persistence/Seed/SeedData.cs).
    private static readonly Guid Spotify = new("d0000000-0000-0000-0000-000000000001");
    private static readonly Guid AppleMusic = new("d0000000-0000-0000-0000-000000000002");

    private readonly ApiFactory _factory;

    public CatalogFlowApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Artist_and_track_lifecycle()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        // Create an artist, then read it back by id and in the list.
        var artistResponse = await client.PostAsJsonAsync(
            "/api/artists", new CreateArtistDto("Flow Test Artist", "flow.test@example.com", "eg"));
        Assert.Equal(HttpStatusCode.Created, artistResponse.StatusCode);
        var artist = (await artistResponse.Content.ReadFromJsonAsync<ArtistDto>())!;
        Assert.Equal("EG", artist.Country);

        Assert.Equal(artist, await client.GetFromJsonAsync<ArtistDto>($"/api/artists/{artist.Id}"));
        Assert.Contains(artist, (await client.GetFromJsonAsync<List<ArtistDto>>("/api/artists"))!);

        // Create a track for that artist: a Draft with no distributions.
        var trackResponse = await client.PostAsJsonAsync("/api/tracks",
            new CreateTrackDto("Flow Test Track", artist.Id, "EG-FLW-26-00001", new DateOnly(2026, 12, 1), "Hip-Hop"));
        Assert.Equal(HttpStatusCode.Created, trackResponse.StatusCode);
        var track = (await trackResponse.Content.ReadFromJsonAsync<TrackDetailDto>())!;
        Assert.Equal("Draft", track.Status);
        Assert.Equal("Flow Test Artist", track.ArtistName);
        Assert.Equal(trackResponse.Headers.Location, new Uri(client.BaseAddress!, $"/api/tracks/{track.Id}"));

        // The list (filtered by artist) carries the artist's name from the batched lookup.
        var byArtist = (await client.GetFromJsonAsync<List<TrackListItemDto>>($"/api/tracks?artistId={artist.Id}"))!;
        var listed = Assert.Single(byArtist);
        Assert.Equal(track.Id, listed.Id);
        Assert.Equal("Flow Test Artist", listed.ArtistName);

        // Distribute: Draft -> Submitted, Pending distributions with DSP names.
        var distributeResponse = await client.PostAsJsonAsync(
            $"/api/tracks/{track.Id}/distribute", new DistributeTrackDto([Spotify, AppleMusic]));
        Assert.Equal(HttpStatusCode.OK, distributeResponse.StatusCode);
        var distributed = (await distributeResponse.Content.ReadFromJsonAsync<TrackDetailDto>())!;
        Assert.Equal("Submitted", distributed.Status);
        Assert.Equal("Flow Test Artist", distributed.ArtistName);
        Assert.Equal(["Apple Music", "Spotify"], distributed.Distributions.Select(d => d.DspName).Order());
        Assert.All(distributed.Distributions, d => Assert.Equal("Pending", d.Status));

        // The change was saved: a fresh GET returns the same state.
        Assert.Equivalent(distributed, await client.GetFromJsonAsync<TrackDetailDto>($"/api/tracks/{track.Id}"));

        // Same DSP again -> 409; Distributed with nothing Live -> 409.
        var again = await client.PostAsJsonAsync($"/api/tracks/{track.Id}/distribute", new DistributeTrackDto([Spotify]));
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);

        var toDistributed = await client.PatchAsJsonAsync($"/api/tracks/{track.Id}/status", new ChangeTrackStatusDto("Distributed"));
        Assert.Equal(HttpStatusCode.Conflict, toDistributed.StatusCode);

        // Submitted -> Draft is allowed and saved.
        var toDraft = await client.PatchAsJsonAsync($"/api/tracks/{track.Id}/status", new ChangeTrackStatusDto("Draft"));
        Assert.Equal(HttpStatusCode.OK, toDraft.StatusCode);
        Assert.Equal("Draft", (await client.GetFromJsonAsync<TrackDetailDto>($"/api/tracks/{track.Id}"))!.Status);
    }

    [Fact]
    public async Task Get_dsps_returns_the_seeded_dsps_sorted_by_name()
    {
        var client = _factory.CreateClient();

        var dsps = await client.GetFromJsonAsync<List<DspDto>>("/api/dsps");

        Assert.Equal(["Apple Music", "Spotify", "YouTube Music"], dsps!.Select(d => d.Name));
        Assert.Contains(dsps!, d => d.Id == Spotify);
    }

    [Theory]
    [InlineData("/api/artists/{0}")]
    [InlineData("/api/tracks/{0}")]
    public async Task Get_unknown_id_returns_404(string urlFormat)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(string.Format(urlFormat, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_unknown_track_returns_404()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PatchAsJsonAsync($"/api/tracks/{Guid.NewGuid()}/status", new ChangeTrackStatusDto("Submitted"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Distribute_to_an_unknown_dsp_returns_404()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var track = await client.GetFromJsonAsync<List<TrackListItemDto>>("/api/tracks?status=draft");
        var unknownDsp = Guid.NewGuid();

        var response = await client.PostAsJsonAsync(
            $"/api/tracks/{track![0].Id}/distribute", new DistributeTrackDto([unknownDsp]));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(unknownDsp.ToString(), await response.Content.ReadAsStringAsync());
    }
}
