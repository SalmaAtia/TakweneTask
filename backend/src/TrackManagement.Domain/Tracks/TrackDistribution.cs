using TrackManagement.Domain.Common;
using TrackManagement.Domain.Common.Exceptions;

namespace TrackManagement.Domain.Tracks;

/// <summary>
/// A track's submission to one DSP. Part of the Track aggregate: created only through <see cref="Track.DistributeTo"/>.
/// </summary>
public sealed class TrackDistribution : Entity
{
    public Guid TrackId { get; private set; }
    public Guid DspId { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public DistributionStatus Status { get; private set; }

    private TrackDistribution()
    {
    }

    internal TrackDistribution(Guid trackId, Guid dspId, DateTimeOffset submittedAtUtc) : base(Guid.NewGuid())
    {
        if (submittedAtUtc.Offset != TimeSpan.Zero)
            throw new BusinessRuleViolationException("SubmittedAt must be in UTC.");

        TrackId = Guard.RequiredId(trackId, "Track id");
        DspId = Guard.RequiredId(dspId, "DSP id");
        SubmittedAt = submittedAtUtc;
        Status = DistributionStatus.Pending;
    }
}
