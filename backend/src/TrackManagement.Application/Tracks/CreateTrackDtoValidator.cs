using FluentValidation;
using TrackManagement.Application.Tracks.Dtos;
using TrackManagement.Shared;

namespace TrackManagement.Application.Tracks;

/// <summary>Request shape only. The domain (Isrc value object, Track) still enforces the real rules.</summary>
public sealed class CreateTrackDtoValidator : AbstractValidator<CreateTrackDto>
{
    private static readonly DateOnly EarliestReleaseDate = new(ValidationConstants.Track.EarliestReleaseYear, 1, 1);

    public CreateTrackDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(ValidationConstants.Track.TitleMaxLength);

        RuleFor(x => x.ArtistId)
            .NotEmpty();

        RuleFor(x => x.Isrc)
            .NotEmpty()
            .Matches(ValidationConstants.Isrc.InputPattern)
            .WithMessage("'Isrc' must be in the format CC-XXX-YY-NNNNN.");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(EarliestReleaseDate);

        RuleFor(x => x.Genre)
            .NotEmpty()
            .MaximumLength(ValidationConstants.Track.GenreMaxLength);
    }
}
