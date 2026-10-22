using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TrackManagement.Api.Auth.Dtos;

namespace TrackManagement.Api.Auth;

public interface IAuthService
{
    /// <returns>The issued token, or null when the credentials are wrong.</returns>
    Task<LoginResponse?> LoginAsync(LoginRequest input, CancellationToken cancellationToken);
}

internal sealed class AuthService : IAuthService
{
    private readonly IValidator<LoginRequest> _validator;
    private readonly IPasswordHasher<DemoUserOptions> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly DemoUserOptions _demoUser;

    public AuthService(
        IValidator<LoginRequest> validator,
        IPasswordHasher<DemoUserOptions> passwordHasher,
        IJwtTokenService jwtTokenService,
        IOptions<DemoUserOptions> demoUser)
    {
        _validator = validator;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _demoUser = demoUser.Value;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest input, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(input, cancellationToken);

        // Always verify the hash, even for an unknown username, so response time doesn't reveal which part was wrong.
        var usernameMatches = string.Equals(input.Username, _demoUser.Username, StringComparison.Ordinal);
        var passwordResult = _passwordHasher.VerifyHashedPassword(_demoUser, _demoUser.PasswordHash, input.Password);

        if (!usernameMatches || passwordResult == PasswordVerificationResult.Failed)
            return null;

        return _jwtTokenService.CreateToken(_demoUser.Username);
    }
}
