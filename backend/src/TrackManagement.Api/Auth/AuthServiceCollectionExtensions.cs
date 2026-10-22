using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TrackManagement.Api.Auth.Dtos;

namespace TrackManagement.Api.Auth;

internal static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<DemoUserOptions>()
            .Bind(configuration.GetSection(DemoUserOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Username), $"{DemoUserOptions.SectionName}:Username is required.")
            .Validate(o => IsBase64(o.PasswordHash),
                $"{DemoUserOptions.SectionName}:PasswordHash is required and must be a PasswordHasher hash.")
            .ValidateOnStart();

        // Bearer options are built from the validated JwtOptions rather than read from config twice.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwt.CreateSigningKey(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub"
                };
            });
        services.AddAuthorization();

        services.AddSingleton<IPasswordHasher<DemoUserOptions>, PasswordHasher<DemoUserOptions>>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    private static bool IsBase64(string value) =>
        !string.IsNullOrWhiteSpace(value) && Convert.TryFromBase64String(value, new byte[value.Length], out _);
}
