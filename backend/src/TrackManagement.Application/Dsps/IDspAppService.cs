using TrackManagement.Application.Dsps.Dtos;

namespace TrackManagement.Application.Dsps;

public interface IDspAppService
{
    Task<IReadOnlyList<DspDto>> GetListAsync(CancellationToken cancellationToken = default);
}
