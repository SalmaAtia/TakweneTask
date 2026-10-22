using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using TrackManagement.Api.Auth;
using TrackManagement.Api.Auth.Dtos;
using TrackManagement.Infrastructure.Persistence;

namespace TrackManagement.Tests.Integration;

/// <summary>
/// Runs the real API against a throwaway SQL Server in Docker (Testcontainers), with the
/// migrations (schema + seed data) applied once. One container is shared by every test in
/// <see cref="ApiCollection"/>, so tests must not depend on data written by other tests.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Username = "test-user";
    public const string Password = "test-password";

    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
        .Build();

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _sqlServer.DisposeAsync();
    }

    /// <summary>A client carrying a bearer token from a real POST /api/auth/login.</summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(Username, Password));
        login.EnsureSuccessStatusCode();
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not Development: user-secrets and appsettings.Development.json are not loaded, so the
        // tests don't depend on (or pick up) anything configured on the developer's machine.
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:DefaultConnection", _sqlServer.GetConnectionString());
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-at-least-32-bytes");
        builder.UseSetting("DemoUser:Username", Username);
        builder.UseSetting("DemoUser:PasswordHash",
            new PasswordHasher<DemoUserOptions>().HashPassword(new DemoUserOptions(), Password));
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Api";
}
