using FluentValidation;
using TrackManagement.Application.Tracks.Dtos;
using TrackManagement.Domain.Tracks;

namespace TrackManagement.Application.Tracks;

public sealed class TrackListQueryDtoValidator : AbstractValidator<TrackListQueryDto>
{
    private static readonly string[] StatusNames = Enum.GetNames<TrackStatus>();

    public TrackListQueryDtoValidator()
    {
        RuleFor(x => x.Genre)
            .MaximumLength(Track.GenreMaxLength);

        // Names only (case-insensitive); numeric values like "1" are rejected.
        RuleFor(x => x.Status)
            .Must(status => StatusNames.Contains(status, StringComparer.OrdinalIgnoreCase))
            .When(x => x.Status is not null)
            .WithMessage($"'Status' must be one of: {string.Join(", ", StatusNames)}.");
    }
}
