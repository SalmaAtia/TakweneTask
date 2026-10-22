using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackManagement.Api.Auth.Dtos;
using TrackManagement.Application.Tracks.Dtos;

namespace TrackManagement.Tests.Integration;

[Collection(ApiCollection.Name)]
public sealed class TracksApiTests
{
    // From the seed data (Infrastructure/Persistence/Seed/SeedData.cs).
    private static readonly Guid NourElSayed = new("a0000000-0000-0000-0000-000000000001");
    private static readonly Guid[] SeededSubmittedTracks =
    [
        new("b0000000-0000-0000-0000-000000000002"), // Tok Tok Anthem
        new("b0000000-0000-0000-0000-000000000008"), // Cedar Heart
        new("b0000000-0000-0000-0000-000000000009"), // Oujda Sunset
    ];

    private readonly ApiFactory _factory;

    public TracksApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_tracks_filtered_by_status_returns_only_matching_tracks()
    {
        var client = _factory.CreateClient();

        // Lowercase on purpose: status names are matched case-insensitively.
        var tracks = await client.GetFromJsonAsync<List<TrackListItemDto>>("/api/tracks?status=submitted");

        Assert.NotNull(tracks);
        Assert.All(tracks, t => Assert.Equal("Submitted", t.Status));
        Assert.Superset(SeededSubmittedTracks.ToHashSet(), tracks.Select(t => t.Id).ToHashSet());
    }

    [Fact]
    public async Task Post_track_without_a_token_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tracks", NewTrack("EG-NES-26-90001"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_track_with_a_duplicate_isrc_returns_409()
    {
        var client = await CreateAuthenticatedClientAsync();

        var created = await client.PostAsJsonAsync("/api/tracks", NewTrack("EG-NES-26-90002"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        // Same ISRC without hyphens and in lowercase: normalized to the same value.
        var duplicate = await client.PostAsJsonAsync("/api/tracks", NewTrack("egnes2690002"));

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("application/problem+json", duplicate.Content.Headers.ContentType?.MediaType);
        var problem = await duplicate.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(StatusCodes.Status409Conflict, problem?.Status);
        Assert.Contains("EG-NES-26-90002", problem?.Detail);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(ApiFactory.Username, ApiFactory.Password));
        login.EnsureSuccessStatusCode();
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private static CreateTrackDto NewTrack(string isrc) =>
        new("Integration Test Track", NourElSayed, isrc, new DateOnly(2026, 12, 1), "Hip-Hop");
}
