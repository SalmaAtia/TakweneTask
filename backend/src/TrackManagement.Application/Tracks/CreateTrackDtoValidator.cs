using FluentValidation;
using TrackManagement.Application.Tracks.Dtos;
using TrackManagement.Domain.Tracks;

namespace TrackManagement.Application.Tracks;

/// <summary>Request shape only. The domain (Isrc value object, Track) still enforces the real rules.</summary>
public sealed class CreateTrackDtoValidator : AbstractValidator<CreateTrackDto>
{
    private static readonly DateOnly EarliestReleaseDate = new(1900, 1, 1);

    public CreateTrackDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(Track.TitleMaxLength);

        RuleFor(x => x.ArtistId)
            .NotEmpty();

        RuleFor(x => x.Isrc)
            .NotEmpty()
            .Matches("^[A-Za-z]{2}-?[A-Za-z0-9]{3}-?[0-9]{2}-?[0-9]{5}$")
            .WithMessage("'Isrc' must be in the format CC-XXX-YY-NNNNN.");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(EarliestReleaseDate);

        RuleFor(x => x.Genre)
            .NotEmpty()
            .MaximumLength(Track.GenreMaxLength);
    }
}
