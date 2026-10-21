using TrackManagement.Application.Artists.Dtos;
using TrackManagement.Domain.Artists;

namespace TrackManagement.Application.Artists;

internal static class ArtistMappings
{
    public static ArtistDto ToDto(this Artist artist) =>
        new(artist.Id, artist.Name, artist.Email.Value, artist.Country.Value);
}
