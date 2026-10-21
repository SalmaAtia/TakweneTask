using FluentValidation;
using TrackManagement.Application.Tracks.Dtos;

namespace TrackManagement.Application.Tracks;

public sealed class ChangeTrackStatusDtoValidator : AbstractValidator<ChangeTrackStatusDto>
{
    public ChangeTrackStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(TrackStatusNames.IsValid)
            .WithMessage(TrackStatusNames.ValidationMessage);
    }
}
