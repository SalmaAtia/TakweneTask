using TrackManagement.Application.Dsps.Dtos;
using TrackManagement.Domain.Dsps;

namespace TrackManagement.Application.Dsps;

/// <summary>Dsp has no domain manager, so this service uses its repository directly.</summary>
public sealed class DspAppService : IDspAppService
{
    private readonly IDspRepository _dsps;

    public DspAppService(IDspRepository dsps)
    {
        _dsps = dsps;
    }

    public async Task<IReadOnlyList<DspDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await _dsps.GetAllAsync(cancellationToken);

        return list.Select(d => new DspDto(d.Id, d.Name)).ToList();
    }
}
