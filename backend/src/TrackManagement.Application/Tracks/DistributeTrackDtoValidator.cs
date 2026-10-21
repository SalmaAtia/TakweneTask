using FluentValidation;
using TrackManagement.Application.Tracks.Dtos;

namespace TrackManagement.Application.Tracks;

public sealed class DistributeTrackDtoValidator : AbstractValidator<DistributeTrackDto>
{
    public DistributeTrackDtoValidator()
    {
        RuleFor(x => x.DspIds)
            .NotEmpty()
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("'Dsp Ids' must not contain duplicates.");

        RuleForEach(x => x.DspIds)
            .NotEmpty();
    }
}
