using TrackManagement.Domain.Common;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Shared.Enums;

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
            throw new DomainException("SubmittedAt must be in UTC.");

        TrackId = Guard.RequiredId(trackId, "Track id");
        DspId = Guard.RequiredId(dspId, "DSP id");
        SubmittedAt = submittedAtUtc;
        Status = DistributionStatus.Pending;
    }

    internal void MarkLive()
    {
        EnsurePending();
        Status = DistributionStatus.Live;
    }

    internal void MarkRejected()
    {
        EnsurePending();
        Status = DistributionStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (Status != DistributionStatus.Pending)
            throw new ConflictException($"Distribution to DSP '{DspId}' is already {Status}.");
    }
}
