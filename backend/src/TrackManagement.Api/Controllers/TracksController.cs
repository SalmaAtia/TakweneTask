using Microsoft.AspNetCore.Mvc;
using TrackManagement.Application.Tracks;
using TrackManagement.Application.Tracks.Dtos;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/tracks")]
[Produces("application/json")]
public sealed class TracksController : ControllerBase
{
    private readonly ITrackAppService _trackAppService;

    public TracksController(ITrackAppService trackAppService)
    {
        _trackAppService = trackAppService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TrackDetailDto>> Create(CreateTrackDto input, CancellationToken cancellationToken)
    {
        var track = await _trackAppService.CreateAsync(input, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = track.Id }, track);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrackListItemDto>>> GetList(
        [FromQuery] Guid? artistId,
        [FromQuery] string? genre,
        [FromQuery] string? status,
        CancellationToken cancellationToken) =>
        Ok(await _trackAppService.GetListAsync(new TrackListQueryDto(artistId, genre, status), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrackDetailDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        await _trackAppService.GetAsync(id, cancellationToken);
}
