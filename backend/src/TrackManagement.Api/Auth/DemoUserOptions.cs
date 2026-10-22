namespace TrackManagement.Api.Auth;

/// <summary>
/// Single demo account. PasswordHash is produced by ASP.NET Core Identity's PasswordHasher
/// (generate one with: dotnet run --project src/TrackManagement.Api -- --hash-password).
/// </summary>
public sealed class DemoUserOptions
{
    public const string SectionName = "DemoUser";

    public string Username { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
}
