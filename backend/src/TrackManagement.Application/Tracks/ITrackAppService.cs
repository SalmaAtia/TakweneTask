using TrackManagement.Application.Tracks.Dtos;

namespace TrackManagement.Application.Tracks;

public interface ITrackAppService
{
    Task<TrackDetailDto> CreateAsync(CreateTrackDto input, CancellationToken cancellationToken = default);

    Task<TrackDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrackListItemDto>> GetListAsync(TrackListQueryDto query, CancellationToken cancellationToken = default);

    Task<TrackDetailDto> DistributeAsync(Guid id, DistributeTrackDto input, CancellationToken cancellationToken = default);

    Task<TrackDetailDto> ChangeStatusAsync(Guid id, ChangeTrackStatusDto input, CancellationToken cancellationToken = default);

    /// <summary>Records the DSP's outcome (Live or Rejected) for the track's Pending distribution to that DSP.</summary>
    Task<TrackDetailDto> ChangeDistributionStatusAsync(
        Guid id, Guid dspId, ChangeDistributionStatusDto input, CancellationToken cancellationToken = default);
}
