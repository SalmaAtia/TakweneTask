using Microsoft.AspNetCore.Mvc;
using TrackManagement.Application.Dsps;
using TrackManagement.Application.Dsps.Dtos;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/dsps")]
[Produces("application/json")]
public sealed class DspsController : ControllerBase
{
    private readonly IDspAppService _dspAppService;

    public DspsController(IDspAppService dspAppService)
    {
        _dspAppService = dspAppService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DspDto>>> GetList(CancellationToken cancellationToken) =>
        Ok(await _dspAppService.GetListAsync(cancellationToken));
}
