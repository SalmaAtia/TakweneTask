using System.Diagnostics;
using Microsoft.OpenApi.Models;
using TrackManagement.Api.Auth;
using TrackManagement.Api.ExceptionHandling;
using TrackManagement.Api.RateLimiting;
using TrackManagement.Api.Swagger;
using TrackManagement.Application;
using TrackManagement.Domain;
using TrackManagement.Infrastructure;

if (args is [PasswordHashCommand.Argument])
{
    PasswordHashCommand.Run();
    return;
}

const string AngularCorsPolicy = "Angular";

var builder = WebApplication.CreateBuilder(args);

// FluentValidation owns request validation; don't let MVC add implicit [Required] for non-nullable strings.
builder.Services.AddControllers(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd(
            "traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
    });
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(AuthorizeOperationFilter.SchemeName, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the accessToken from POST /api/auth/login."
    });
    options.OperationFilter<AuthorizeOperationFilter>();
});
builder.Services.AddHealthChecks();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddLoginRateLimiting(builder.Configuration);

var corsOrigin = builder.Configuration["Cors:AllowedOrigin"];
if (string.IsNullOrWhiteSpace(corsOrigin))
    throw new InvalidOperationException("Cors:AllowedOrigin is required.");
builder.Services.AddCors(options => options.AddPolicy(AngularCorsPolicy, policy => policy
    .WithOrigins(corsOrigin)
    .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Patch)
    .WithHeaders("Authorization", "Content-Type")));

builder.Services
    .AddDomain()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Swagger is always on in Development. Elsewhere it is opt-in (Swagger:Enabled); the production
// deployment turns it on on purpose so reviewers can try the API.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(AngularCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Exposes the generated Program class to WebApplicationFactory<Program> in the integration tests.
public partial class Program;
