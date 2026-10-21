using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TrackManagement.Application.Artists;

namespace TrackManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IArtistAppService, ArtistAppService>();

        return services;
    }
}
