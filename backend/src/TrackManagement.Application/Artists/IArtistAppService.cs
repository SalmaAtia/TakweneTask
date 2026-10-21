using TrackManagement.Application.Artists.Dtos;

namespace TrackManagement.Application.Artists;

public interface IArtistAppService
{
    Task<ArtistDto> CreateAsync(CreateArtistDto input, CancellationToken cancellationToken = default);

    Task<ArtistDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArtistDto>> GetListAsync(CancellationToken cancellationToken = default);
}
