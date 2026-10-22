using FluentValidation;
using TrackManagement.Application.Tracks.Dtos;

namespace TrackManagement.Application.Tracks;

public sealed class ChangeDistributionStatusDtoValidator : AbstractValidator<ChangeDistributionStatusDto>
{
    public ChangeDistributionStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(DistributionOutcomeNames.IsValid)
            .WithMessage(DistributionOutcomeNames.ValidationMessage);
    }
}
