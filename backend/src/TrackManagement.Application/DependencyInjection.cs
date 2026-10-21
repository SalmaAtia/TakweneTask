using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrackManagement.Application.Artists;
using TrackManagement.Application.Tracks;

namespace TrackManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IArtistAppService, ArtistAppService>();
        services.AddScoped<ITrackAppService, TrackAppService>();

        return services;
    }
}
