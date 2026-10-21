using TrackManagement.Domain.Common;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Tracks;

public sealed class Track : Entity, IAggregateRoot
{
    public const int TitleMaxLength = 200;
    public const int GenreMaxLength = 50;

    private readonly List<TrackDistribution> _distributions = [];

    public string Title { get; private set; } = null!;
    public Guid ArtistId { get; private set; }
    public Isrc Isrc { get; private set; } = null!;
    public DateOnly ReleaseDate { get; private set; }
    public string Genre { get; private set; } = null!;
    public TrackStatus Status { get; private set; }

    public IReadOnlyCollection<TrackDistribution> Distributions => _distributions.AsReadOnly();

    private Track()
    {
    }

    private Track(Guid id, string title, Guid artistId, Isrc isrc, DateOnly releaseDate, string genre) : base(id)
    {
        Title = title;
        ArtistId = artistId;
        Isrc = isrc;
        ReleaseDate = releaseDate;
        Genre = genre;
        Status = TrackStatus.Draft;
    }

    /// <summary>Use <see cref="TrackManager.CreateAsync"/>; it checks the artist exists and the ISRC is unique.</summary>
    internal static Track Create(string title, Guid artistId, Isrc isrc, DateOnly releaseDate, string genre)
    {
        ArgumentNullException.ThrowIfNull(isrc);

        return new Track(
            Guid.NewGuid(),
            Guard.RequiredText(title, "Title", TitleMaxLength),
            Guard.RequiredId(artistId, "Artist id"),
            isrc,
            releaseDate,
            Guard.RequiredText(genre, "Genre", GenreMaxLength));
    }

    // Stub: full distribution rules (e.g. allowed track statuses) come in a later commit.
    public TrackDistribution DistributeTo(Guid dspId, DateTimeOffset submittedAtUtc)
    {
        if (_distributions.Any(d => d.DspId == dspId))
            throw new BusinessRuleViolationException("Track has already been distributed to this DSP.");

        var distribution = new TrackDistribution(Id, dspId, submittedAtUtc);
        _distributions.Add(distribution);

        return distribution;
    }

    // Stub: forward-only, one step at a time (Draft -> Submitted -> Distributed).
    public void ChangeStatus(TrackStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
            throw new BusinessRuleViolationException($"Unknown track status '{newStatus}'.");

        if (newStatus != Status + 1)
            throw new BusinessRuleViolationException($"Cannot change track status from {Status} to {newStatus}.");

        Status = newStatus;
    }
}
