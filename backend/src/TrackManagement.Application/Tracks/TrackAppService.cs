using FluentValidation;
using TrackManagement.Application.Tracks.Dtos;
using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Common;
using TrackManagement.Domain.Dsps;
using TrackManagement.Domain.Tracks;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Application.Tracks;

public sealed class TrackAppService : ITrackAppService
{
    private static readonly IReadOnlyDictionary<Guid, string> NoDspNames = new Dictionary<Guid, string>();

    private readonly IValidator<CreateTrackDto> _createValidator;
    private readonly IValidator<TrackListQueryDto> _listQueryValidator;
    private readonly IValidator<DistributeTrackDto> _distributeValidator;
    private readonly IValidator<ChangeTrackStatusDto> _changeStatusValidator;
    private readonly TrackManager _trackManager;
    private readonly ArtistManager _artistManager;
    private readonly IDspRepository _dsps;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public TrackAppService(
        IValidator<CreateTrackDto> createValidator,
        IValidator<TrackListQueryDto> listQueryValidator,
        IValidator<DistributeTrackDto> distributeValidator,
        IValidator<ChangeTrackStatusDto> changeStatusValidator,
        TrackManager trackManager,
        ArtistManager artistManager,
        IDspRepository dsps,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _createValidator = createValidator;
        _listQueryValidator = listQueryValidator;
        _distributeValidator = distributeValidator;
        _changeStatusValidator = changeStatusValidator;
        _trackManager = trackManager;
        _artistManager = artistManager;
        _dsps = dsps;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // A new track is a Draft with no distributions, so there are no DSP names to load.
        return track.ToDetailDto(NoDspNames);
    }

    public async Task<TrackDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var track = await _trackManager.GetAsync(id, cancellationToken);

        return await ToDetailDtoAsync(track, cancellationToken);
    }

    public async Task<IReadOnlyList<TrackListItemDto>> GetListAsync(
        TrackListQueryDto query, CancellationToken cancellationToken = default)
    {
        await _listQueryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var filter = new TrackFilter(
            query.ArtistId,
            query.Genre,
            query.Status is null ? null : TrackStatusNames.Parse(query.Status));

        var tracks = await _trackManager.GetListAsync(filter, cancellationToken);

        // One batched lookup for all artists on the page (no per-track queries).
        var artistIds = tracks.Select(t => t.ArtistId).Distinct().ToList();
        var artists = await _artistManager.GetByIdsAsync(artistIds, cancellationToken);
        var artistNames = artists.ToDictionary(a => a.Id, a => a.Name);

        return tracks.Select(t => t.ToListItemDto(artistNames[t.ArtistId])).ToList();
    }

    public async Task<TrackDetailDto> DistributeAsync(
        Guid id, DistributeTrackDto input, CancellationToken cancellationToken = default)
    {
        await _distributeValidator.ValidateAndThrowAsync(input, cancellationToken);

        var track = await _trackManager.GetForUpdateAsync(id, cancellationToken);

        await _trackManager.DistributeAsync(
            track, input.DspIds, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDetailDtoAsync(track, cancellationToken);
    }

    public async Task<TrackDetailDto> ChangeStatusAsync(
        Guid id, ChangeTrackStatusDto input, CancellationToken cancellationToken = default)
    {
        await _changeStatusValidator.ValidateAndThrowAsync(input, cancellationToken);

        var track = await _trackManager.GetForUpdateAsync(id, cancellationToken);

        track.ChangeStatus(TrackStatusNames.Parse(input.Status));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDetailDtoAsync(track, cancellationToken);
    }

    // One batched lookup for the names of every DSP the track is distributed to.
    private async Task<TrackDetailDto> ToDetailDtoAsync(Track track, CancellationToken cancellationToken)
    {
        var dspIds = track.Distributions.Select(d => d.DspId).Distinct().ToList();
        var dsps = await _dsps.GetByIdsAsync(dspIds, cancellationToken);

        return track.ToDetailDto(dsps.ToDictionary(d => d.Id, d => d.Name));
    }
}
