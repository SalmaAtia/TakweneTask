using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace TrackManagement.Api.RateLimiting;

internal static class RateLimitingServiceCollectionExtensions
{
    /// <summary>Policy name for <c>[EnableRateLimiting]</c> on the login action.</summary>
    public const string LoginPolicy = "login";

    /// <summary>
    /// The demo credentials are public, so login is limited per client IP to slow down password guessing.
    /// Behind a reverse proxy the client IP comes from X-Forwarded-For (ASPNETCORE_FORWARDEDHEADERS_ENABLED).
    /// </summary>
    public static IServiceCollection AddLoginRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<LoginRateLimitOptions>()
            .Bind(configuration.GetSection(LoginRateLimitOptions.SectionName))
            .Validate(o => o.PermitLimit > 0, $"{LoginRateLimitOptions.SectionName}:PermitLimit must be greater than 0.")
            .Validate(o => o.WindowSeconds > 0, $"{LoginRateLimitOptions.SectionName}:WindowSeconds must be greater than 0.")
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteRejectionAsync;

            options.AddPolicy(LoginPolicy, httpContext =>
            {
                var limit = httpContext.RequestServices.GetRequiredService<IOptions<LoginRateLimitOptions>>().Value;
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limit.PermitLimit,
                    Window = TimeSpan.FromSeconds(limit.WindowSeconds),
                    QueueLimit = 0
                });
            });
        });

        return services;
    }

    private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        var detail = "Too many login attempts.";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
            httpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
            detail += $" Try again in {seconds} seconds.";
        }

        await httpContext.RequestServices.GetRequiredService<IProblemDetailsService>().TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Detail = detail
                }
            });
    }
}
