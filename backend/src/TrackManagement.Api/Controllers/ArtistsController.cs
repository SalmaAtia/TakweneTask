using Microsoft.AspNetCore.Mvc;
using TrackManagement.Application.Artists;
using TrackManagement.Application.Artists.Dtos;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/artists")]
[Produces("application/json")]
public sealed class ArtistsController : ControllerBase
{
    private readonly IArtistAppService _artistAppService;

    public ArtistsController(IArtistAppService artistAppService)
    {
        _artistAppService = artistAppService;
    }

    [HttpPost]
    [ProducesResponseType<ArtistDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ArtistDto>> Create(CreateArtistDto input, CancellationToken cancellationToken)
    {
        var artist = await _artistAppService.CreateAsync(input, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = artist.Id }, artist);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ArtistDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtistDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        await _artistAppService.GetAsync(id, cancellationToken);

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ArtistDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ArtistDto>>> GetList(CancellationToken cancellationToken) =>
        Ok(await _artistAppService.GetListAsync(cancellationToken));
}
