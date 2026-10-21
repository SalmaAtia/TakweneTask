using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Tracks;

namespace TrackManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Domain managers are registered here because Domain has no DI dependency.
        services.AddScoped<ArtistManager>();
        services.AddScoped<TrackManager>();

        return services;
    }
}
