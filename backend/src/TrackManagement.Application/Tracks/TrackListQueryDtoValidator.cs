using FluentValidation;
using TrackManagement.Application.Tracks.Dtos;
using TrackManagement.Domain.Tracks;

namespace TrackManagement.Application.Tracks;

public sealed class TrackListQueryDtoValidator : AbstractValidator<TrackListQueryDto>
{
    public TrackListQueryDtoValidator()
    {
        RuleFor(x => x.Genre)
            .MaximumLength(Track.GenreMaxLength);

        RuleFor(x => x.Status)
            .Must(TrackStatusNames.IsValid)
            .When(x => x.Status is not null)
            .WithMessage(TrackStatusNames.ValidationMessage);
    }
}
