using TrackManagement.Domain.Common;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Domain.Artists;

public sealed class Artist : Entity, IAggregateRoot
{
    public const int NameMaxLength = 200;

    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public CountryCode Country { get; private set; } = null!;

    private Artist()
    {
    }

    private Artist(Guid id, string name, Email email, CountryCode country) : base(id)
    {
        Name = name;
        Email = email;
        Country = country;
    }

    /// <summary>Use <see cref="ArtistManager.CreateAsync"/>; it enforces email uniqueness.</summary>
    internal static Artist Create(string name, Email email, CountryCode country)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(country);

        return new Artist(Guid.NewGuid(), Guard.RequiredText(name, "Artist name", NameMaxLength), email, country);
    }

    public void Rename(string name) => Name = Guard.RequiredText(name, "Artist name", NameMaxLength);

    /// <summary>Use <see cref="ArtistManager.ChangeEmailAsync"/>; it enforces email uniqueness.</summary>
    internal void ChangeEmail(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);
        Email = email;
    }

    public void ChangeCountry(CountryCode country)
    {
        ArgumentNullException.ThrowIfNull(country);
        Country = country;
    }
}
