using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.Dsps;
using TrackManagement.Domain.Tracks;
using TrackManagement.Domain.ValueObjects;
using TrackManagement.Tests.Fakes;

namespace TrackManagement.Tests.Domain;

public sealed class TrackManagerTests
{
    private static readonly DateTime UtcNow = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly ReleaseDate = new(2025, 3, 14);

    private readonly InMemoryTrackRepository _tracks = new();
    private readonly InMemoryArtistRepository _artists = new();
    private readonly InMemoryDspRepository _dsps = new();
    private readonly TrackManager _manager;
    private readonly Artist _artist;
    private readonly Dsp _spotify;

    public TrackManagerTests()
    {
        _manager = new TrackManager(_tracks, _artists, _dsps);

        _artist = Artist.Create("Nour El Sayed", Email.Create("nour@example.com"), CountryCode.Create("EG"));
        _artists.Items.Add(_artist);

        _spotify = Dsp.Create("Spotify");
        _dsps.Items.Add(_spotify);
    }

    [Fact]
    public async Task CreateAsync_registers_a_draft_track_in_the_repository()
    {
        var track = await _manager.CreateAsync("Shobra Nights", _artist.Id, Isrc.Create("EG-NES-25-00001"), ReleaseDate, "Mahraganat");

        Assert.Equal(TrackStatus.Draft, track.Status);
        Assert.Same(track, Assert.Single(_tracks.Items));
    }

    [Fact]
    public async Task CreateAsync_rejects_a_duplicate_isrc()
    {
        await _manager.CreateAsync("Shobra Nights", _artist.Id, Isrc.Create("EG-NES-25-00001"), ReleaseDate, "Mahraganat");

        // Same ISRC in a different input form normalizes to the same value.
        await Assert.ThrowsAsync<ConflictException>(() =>
            _manager.CreateAsync("Another", _artist.Id, Isrc.Create("egnes2500001"), ReleaseDate, "Pop"));
        Assert.Single(_tracks.Items);
    }

    [Fact]
    public async Task CreateAsync_rejects_an_unknown_artist()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _manager.CreateAsync("Shobra Nights", Guid.NewGuid(), Isrc.Create("EG-NES-25-00001"), ReleaseDate, "Mahraganat"));
        Assert.Empty(_tracks.Items);
    }

    [Fact]
    public async Task DistributeAsync_adds_distributions_for_known_dsps()
    {
        var track = await NewTrackAsync();

        await _manager.DistributeAsync(track, [_spotify.Id], UtcNow);

        Assert.Equal(_spotify.Id, Assert.Single(track.Distributions).DspId);
        Assert.Equal(TrackStatus.Submitted, track.Status);
    }

    [Fact]
    public async Task DistributeAsync_rejects_unknown_dsp_ids_and_names_them()
    {
        var track = await NewTrackAsync();
        var unknown = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _manager.DistributeAsync(track, [_spotify.Id, unknown], UtcNow));

        Assert.Contains(unknown.ToString(), ex.Message);
        Assert.DoesNotContain(_spotify.Id.ToString(), ex.Message);
        Assert.Empty(track.Distributions);
        Assert.Equal(TrackStatus.Draft, track.Status);
    }

    private Task<Track> NewTrackAsync() =>
        _manager.CreateAsync("Shobra Nights", _artist.Id, Isrc.Create("EG-NES-25-00001"), ReleaseDate, "Mahraganat");
}
