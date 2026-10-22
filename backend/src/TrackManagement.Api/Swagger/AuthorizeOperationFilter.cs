using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TrackManagement.Api.Swagger;

/// <summary>Marks only [Authorize] endpoints as needing the Bearer token, so public GETs show no padlock.</summary>
internal sealed class AuthorizeOperationFilter : IOperationFilter
{
    public const string SchemeName = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (!metadata.OfType<IAuthorizeData>().Any() || metadata.OfType<IAllowAnonymous>().Any())
            return;

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = SchemeName }
        };
        operation.Security.Add(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
    }
}
