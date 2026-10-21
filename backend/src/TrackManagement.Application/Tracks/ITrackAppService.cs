using TrackManagement.Application.Tracks.Dtos;

namespace TrackManagement.Application.Tracks;

public interface ITrackAppService
{
    Task<TrackDetailDto> CreateAsync(CreateTrackDto input, CancellationToken cancellationToken = default);

    Task<TrackDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrackListItemDto>> GetListAsync(TrackListQueryDto query, CancellationToken cancellationToken = default);

    Task<TrackDetailDto> DistributeAsync(Guid id, DistributeTrackDto input, CancellationToken cancellationToken = default);

    Task<TrackDetailDto> ChangeStatusAsync(Guid id, ChangeTrackStatusDto input, CancellationToken cancellationToken = default);
}
