using FluentValidation;
using TrackManagement.Application.Artists.Dtos;
using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Common;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Application.Artists;

public sealed class ArtistAppService : IArtistAppService
{
    private readonly IValidator<CreateArtistDto> _createValidator;
    private readonly ArtistManager _artistManager;
    private readonly IArtistRepository _artists;
    private readonly IUnitOfWork _unitOfWork;

    public ArtistAppService(
        IValidator<CreateArtistDto> createValidator,
        ArtistManager artistManager,
        IArtistRepository artists,
        IUnitOfWork unitOfWork)
    {
        _createValidator = createValidator;
        _artistManager = artistManager;
        _artists = artists;
        _unitOfWork = unitOfWork;
    }

    public async Task<ArtistDto> CreateAsync(CreateArtistDto input, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(input, cancellationToken);

        var artist = await _artistManager.CreateAsync(
            input.Name,
            Email.Create(input.Email),
            CountryCode.Create(input.Country),
            cancellationToken);

        await _artists.AddAsync(artist, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return artist.ToDto();
    }

    public async Task<ArtistDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var artist = await _artists.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), id);

        return artist.ToDto();
    }

    public async Task<IReadOnlyList<ArtistDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await _artists.GetAllAsync(cancellationToken);

        return list.Select(a => a.ToDto()).ToList();
    }
}
