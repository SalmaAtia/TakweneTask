using Microsoft.AspNetCore.Mvc;
using TrackManagement.Api.Auth;
using TrackManagement.Api.Auth.Dtos;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest input, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(input, cancellationToken);

        return response is null
            ? Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Invalid username or password.")
            : response;
    }
}
