using FluentValidation;
using TrackManagement.Api.Auth.Dtos;
using TrackManagement.Shared;

namespace TrackManagement.Api.Auth;

internal sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(AuthValidationConstants.UsernameMaxLength);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(AuthValidationConstants.PasswordMaxLength);
    }
}
