using TrackManagement.Shared.Enums;

namespace TrackManagement.Domain.Tracks;

/// <summary>Optional criteria for listing tracks. Null means "don't filter on this".</summary>
public sealed record TrackFilter(Guid? ArtistId = null, string? Genre = null, TrackStatus? Status = null);
