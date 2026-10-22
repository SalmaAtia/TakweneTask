using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.Tracks;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Tests.Domain;

public sealed class TrackTests
{
    private static readonly DateTime UtcNow = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Spotify = Guid.NewGuid();
    private static readonly Guid AppleMusic = Guid.NewGuid();

    [Fact]
    public void Create_starts_as_draft_with_no_distributions()
    {
        var track = NewTrack();

        Assert.Equal(TrackStatus.Draft, track.Status);
        Assert.Empty(track.Distributions);
    }

    // ---- Status transitions ----

    [Fact]
    public void Draft_can_be_submitted()
    {
        var track = NewTrack();

        track.ChangeStatus(TrackStatus.Submitted);

        Assert.Equal(TrackStatus.Submitted, track.Status);
    }

    [Fact]
    public void Submitted_can_go_back_to_draft()
    {
        var track = SubmittedTrack();

        track.ChangeStatus(TrackStatus.Draft);

        Assert.Equal(TrackStatus.Draft, track.Status);
    }

    [Fact]
    public void Submitted_with_a_live_distribution_can_become_distributed()
    {
        var track = SubmittedTrack();
        track.MarkDistributionLive(Spotify);

        track.ChangeStatus(TrackStatus.Distributed);

        Assert.Equal(TrackStatus.Distributed, track.Status);
    }

    [Theory]
    [InlineData(TrackStatus.Draft)]
    [InlineData(TrackStatus.Distributed)]
    public void Draft_rejects_invalid_transitions(TrackStatus target)
    {
        var track = NewTrack();

        Assert.Throws<ConflictException>(() => track.ChangeStatus(target));
        Assert.Equal(TrackStatus.Draft, track.Status);
    }

    [Fact]
    public void Submitted_cannot_be_submitted_again()
    {
        var track = SubmittedTrack();

        Assert.Throws<ConflictException>(() => track.ChangeStatus(TrackStatus.Submitted));
    }

    [Theory]
    [InlineData(TrackStatus.Draft)]
    [InlineData(TrackStatus.Submitted)]
    [InlineData(TrackStatus.Distributed)]
    public void Distributed_is_final(TrackStatus target)
    {
        var track = DistributedTrack();

        Assert.Throws<ConflictException>(() => track.ChangeStatus(target));
        Assert.Equal(TrackStatus.Distributed, track.Status);
    }

    [Fact]
    public void Unknown_status_value_is_invalid_input()
    {
        var track = NewTrack();

        Assert.Throws<DomainException>(() => track.ChangeStatus((TrackStatus)42));
    }

    // ---- Distributed requires a Live distribution ----

    [Fact]
    public void Distributed_requires_a_live_distribution_when_all_are_pending()
    {
        var track = SubmittedTrack();

        Assert.Throws<ConflictException>(() => track.ChangeStatus(TrackStatus.Distributed));
        Assert.Equal(TrackStatus.Submitted, track.Status);
    }

    [Fact]
    public void Distributed_requires_a_live_distribution_when_all_are_rejected()
    {
        var track = SubmittedTrack();
        track.MarkDistributionRejected(Spotify);

        Assert.Throws<ConflictException>(() => track.ChangeStatus(TrackStatus.Distributed));
    }

    [Fact]
    public void Distributed_requires_a_live_distribution_when_submitted_without_any()
    {
        var track = NewTrack();
        track.ChangeStatus(TrackStatus.Submitted);

        Assert.Throws<ConflictException>(() => track.ChangeStatus(TrackStatus.Distributed));
    }

    // ---- DistributeTo ----

    [Fact]
    public void DistributeTo_adds_pending_distributions_and_moves_draft_to_submitted()
    {
        var track = NewTrack();

        track.DistributeTo([Spotify, AppleMusic], UtcNow);

        Assert.Equal(TrackStatus.Submitted, track.Status);
        Assert.Equal(2, track.Distributions.Count);
        Assert.All(track.Distributions, d =>
        {
            Assert.Equal(DistributionStatus.Pending, d.Status);
            Assert.Equal(track.Id, d.TrackId);
            Assert.Equal(new DateTimeOffset(UtcNow), d.SubmittedAt);
        });
    }

    [Fact]
    public void DistributeTo_a_new_dsp_keeps_a_submitted_track_submitted()
    {
        var track = SubmittedTrack();

        track.DistributeTo([AppleMusic], UtcNow);

        Assert.Equal(TrackStatus.Submitted, track.Status);
        Assert.Equal(2, track.Distributions.Count);
    }

    [Fact]
    public void DistributeTo_rejects_a_dsp_the_track_is_already_distributed_to_and_adds_nothing()
    {
        var track = SubmittedTrack();

        // AppleMusic is new, Spotify is a duplicate: the whole call is rejected.
        Assert.Throws<ConflictException>(() => track.DistributeTo([AppleMusic, Spotify], UtcNow));
        Assert.Single(track.Distributions);
    }

    [Fact]
    public void DistributeTo_rejects_the_same_dsp_twice_in_one_request()
    {
        var track = NewTrack();

        Assert.Throws<DomainException>(() => track.DistributeTo([Spotify, Spotify], UtcNow));
        Assert.Empty(track.Distributions);
        Assert.Equal(TrackStatus.Draft, track.Status);
    }

    [Fact]
    public void DistributeTo_rejects_an_already_distributed_track()
    {
        var track = DistributedTrack();

        Assert.Throws<ConflictException>(() => track.DistributeTo([AppleMusic], UtcNow));
    }

    [Fact]
    public void DistributeTo_rejects_an_empty_dsp_list()
    {
        var track = NewTrack();

        Assert.Throws<DomainException>(() => track.DistributeTo([], UtcNow));
    }

    [Fact]
    public void DistributeTo_rejects_a_non_utc_timestamp()
    {
        var track = NewTrack();
        var local = DateTime.SpecifyKind(UtcNow, DateTimeKind.Local);

        Assert.Throws<DomainException>(() => track.DistributeTo([Spotify], local));
    }

    // ---- Helpers ----

    private static Track NewTrack() =>
        Track.Create("Shobra Nights", Guid.NewGuid(), Isrc.Create("EG-NES-25-00001"), new DateOnly(2025, 3, 14), "Mahraganat");

    /// <summary>Submitted with one Pending distribution (Spotify).</summary>
    private static Track SubmittedTrack()
    {
        var track = NewTrack();
        track.DistributeTo([Spotify], UtcNow);
        return track;
    }

    private static Track DistributedTrack()
    {
        var track = SubmittedTrack();
        track.MarkDistributionLive(Spotify);
        track.ChangeStatus(TrackStatus.Distributed);
        return track;
    }
}
