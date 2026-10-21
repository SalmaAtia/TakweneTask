using Microsoft.Extensions.DependencyInjection;
using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Tracks;

namespace TrackManagement.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<ArtistManager>();
        services.AddScoped<TrackManager>();

        return services;
    }
}
