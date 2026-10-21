using TrackManagement.Domain.Common;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Tracks;

public sealed class Track : Entity, IAggregateRoot
{
    public const int TitleMaxLength = 200;
    public const int GenreMaxLength = 50;

    private static readonly HashSet<(TrackStatus From, TrackStatus To)> AllowedTransitions =
    [
        (TrackStatus.Draft, TrackStatus.Submitted),
        (TrackStatus.Submitted, TrackStatus.Distributed),
        (TrackStatus.Submitted, TrackStatus.Draft),
    ];

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

    /// <summary>
    /// Submits the track to new DSPs as Pending distributions. All-or-nothing: if any DSP is
    /// already present the whole call is rejected and nothing is added. Draft moves to Submitted.
    /// </summary>
    public void DistributeTo(IReadOnlyCollection<Guid> dspIds, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(dspIds);

        if (dspIds.Count == 0)
            throw new DomainException("At least one DSP is required.");

        if (dspIds.Contains(Guid.Empty))
            throw new DomainException("DSP id must not be empty.");

        if (dspIds.Distinct().Count() != dspIds.Count)
            throw new DomainException("DSP ids must be unique.");

        if (utcNow.Kind != DateTimeKind.Utc)
            throw new DomainException("The distribution timestamp must be in UTC.");

        if (Status == TrackStatus.Distributed)
            throw new ConflictException("Track is already distributed.");

        var alreadyDistributed = dspIds.Where(id => _distributions.Any(d => d.DspId == id)).ToList();
        if (alreadyDistributed.Count > 0)
            throw new ConflictException(
                $"Track has already been distributed to DSP(s): {string.Join(", ", alreadyDistributed)}.");

        var submittedAt = new DateTimeOffset(utcNow);
        foreach (var dspId in dspIds)
            _distributions.Add(new TrackDistribution(Id, dspId, submittedAt));

        if (Status == TrackStatus.Draft)
            Status = TrackStatus.Submitted;
    }

    public void ChangeStatus(TrackStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
            throw new DomainException($"Unknown track status '{newStatus}'.");

        if (!AllowedTransitions.Contains((Status, newStatus)))
            throw new ConflictException($"Cannot change track status from {Status} to {newStatus}.");

        if (newStatus == TrackStatus.Distributed && !_distributions.Any(d => d.Status == DistributionStatus.Live))
            throw new ConflictException("A track can only become Distributed once at least one distribution is Live.");

        Status = newStatus;
    }

    public void MarkDistributionLive(Guid dspId) => GetDistribution(dspId).MarkLive();

    public void MarkDistributionRejected(Guid dspId) => GetDistribution(dspId).MarkRejected();

    private TrackDistribution GetDistribution(Guid dspId) =>
        _distributions.SingleOrDefault(d => d.DspId == dspId)
        ?? throw new NotFoundException($"Track has not been distributed to DSP '{dspId}'.");
}
