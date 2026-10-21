using TrackManagement.Application.Tracks.Dtos;

namespace TrackManagement.Application.Tracks;

public interface ITrackAppService
{
    Task<TrackDetailDto> CreateAsync(CreateTrackDto input, CancellationToken cancellationToken = default);

    Task<TrackDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrackListItemDto>> GetListAsync(TrackListQueryDto query, CancellationToken cancellationToken = default);
}
