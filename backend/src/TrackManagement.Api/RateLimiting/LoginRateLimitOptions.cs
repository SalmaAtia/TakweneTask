namespace TrackManagement.Api.RateLimiting;

/// <summary>Fixed-window limit on POST /api/auth/login, per client IP.</summary>
public sealed class LoginRateLimitOptions
{
    public const string SectionName = "RateLimiting:Login";

    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
}
