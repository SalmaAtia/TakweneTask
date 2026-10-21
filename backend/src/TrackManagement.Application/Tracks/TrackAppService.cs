using FluentValidation;
using TrackManagement.Application.Tracks.Dtos;
using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Common;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.Dsps;
using TrackManagement.Domain.Tracks;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Application.Tracks;

public sealed class TrackAppService : ITrackAppService
{
    private static readonly IReadOnlyDictionary<Guid, string> NoDspNames = new Dictionary<Guid, string>();

    private readonly IValidator<CreateTrackDto> _createValidator;
    private readonly IValidator<TrackListQueryDto> _listQueryValidator;
    private readonly TrackManager _trackManager;
    private readonly ITrackRepository _tracks;
    private readonly IArtistRepository _artists;
    private readonly IDspRepository _dsps;
    private readonly IUnitOfWork _unitOfWork;

    public TrackAppService(
        IValidator<CreateTrackDto> createValidator,
        IValidator<TrackListQueryDto> listQueryValidator,
        TrackManager trackManager,
        ITrackRepository tracks,
        IArtistRepository artists,
        IDspRepository dsps,
        IUnitOfWork unitOfWork)
    {
        _createValidator = createValidator;
        _listQueryValidator = listQueryValidator;
        _trackManager = trackManager;
        _tracks = tracks;
        _artists = artists;
        _dsps = dsps;
        _unitOfWork = unitOfWork;
    }

    public async Task<TrackDetailDto> CreateAsync(CreateTrackDto input, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(input, cancellationToken);

        var track = await _trackManager.CreateAsync(
            input.Title,
            input.ArtistId,
            Isrc.Create(input.Isrc),
            input.ReleaseDate,
            input.Genre,
            cancellationToken);

        await _tracks.AddAsync(track, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // A new track is a Draft with no distributions, so there are no DSP names to load.
        return track.ToDetailDto(NoDspNames);
    }

    public async Task<TrackDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var track = await _tracks.GetWithDistributionsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Track), id);

        var dspIds = track.Distributions.Select(d => d.DspId).Distinct().ToList();
        var dsps = await _dsps.GetByIdsAsync(dspIds, cancellationToken);

        return track.ToDetailDto(dsps.ToDictionary(d => d.Id, d => d.Name));
    }

    public async Task<IReadOnlyList<TrackListItemDto>> GetListAsync(
        TrackListQueryDto query, CancellationToken cancellationToken = default)
    {
        await _listQueryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var filter = new TrackFilter(
            query.ArtistId,
            query.Genre,
            query.Status is null ? null : Enum.Parse<TrackStatus>(query.Status, ignoreCase: true));

        var tracks = await _tracks.GetListAsync(filter, cancellationToken);

        // One batched lookup for all artists on the page (no per-track queries).
        var artistIds = tracks.Select(t => t.ArtistId).Distinct().ToList();
        var artists = await _artists.GetByIdsAsync(artistIds, cancellationToken);
        var artistNames = artists.ToDictionary(a => a.Id, a => a.Name);

        return tracks.Select(t => t.ToListItemDto(artistNames[t.ArtistId])).ToList();
    }
}
