using FluentValidation;
using TrackManagement.Application.Artists.Dtos;
using TrackManagement.Shared;

namespace TrackManagement.Application.Artists;

/// <summary>Request shape only. The domain (value objects, Artist) still enforces the real rules.</summary>
public sealed class CreateArtistDtoValidator : AbstractValidator<CreateArtistDto>
{
    public CreateArtistDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.Artist.NameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(ValidationConstants.Email.MaxLength)
            .EmailAddress();

        RuleFor(x => x.Country)
            .NotEmpty()
            .Matches(ValidationConstants.CountryCode.InputPattern)
            .WithMessage("'Country' must be a 2-letter ISO 3166-1 alpha-2 code.");
    }
}
