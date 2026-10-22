using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackManagement.Api.Auth.Dtos;

namespace TrackManagement.Tests.Integration;

[Collection(ApiCollection.Name)]
public sealed class AuthApiTests
{
    private readonly ApiFactory _factory;

    public AuthApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_is_rate_limited_after_the_permit_limit()
    {
        // Own host with a low limit (same database), so the shared client's limit is untouched.
        using var limited = _factory.WithWebHostBuilder(builder =>
            builder.UseSetting("RateLimiting:Login:PermitLimit", "2"));
        var client = limited.CreateClient();
        var wrongPassword = new LoginRequest(ApiFactory.Username, "wrong-password");

        for (var i = 0; i < 2; i++)
        {
            var allowed = await client.PostAsJsonAsync("/api/auth/login", wrongPassword);
            Assert.Equal(HttpStatusCode.Unauthorized, allowed.StatusCode);
        }

        // The limit counts every attempt, so even the right password is now refused.
        var rejected = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(ApiFactory.Username, ApiFactory.Password));

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.True(rejected.Headers.RetryAfter?.Delta > TimeSpan.Zero);
        var problem = await rejected.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(StatusCodes.Status429TooManyRequests, problem?.Status);
    }
}
