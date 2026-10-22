using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ArtistDto>> Create(CreateArtistDto input, CancellationToken cancellationToken)
    {
        var artist = await _artistAppService.CreateAsync(input, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = artist.Id }, artist);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArtistDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        await _artistAppService.GetAsync(id, cancellationToken);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ArtistDto>>> GetList(CancellationToken cancellationToken) =>
        Ok(await _artistAppService.GetListAsync(cancellationToken));
}
