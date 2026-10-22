using TrackManagement.Domain.Common;
using TrackManagement.Shared;

namespace TrackManagement.Domain.Dsps;

/// <summary>Digital Service Provider (e.g. Spotify, Anghami).</summary>
public sealed class Dsp : Entity, IAggregateRoot
{
    public string Name { get; private set; } = null!;

    private Dsp()
    {
    }

    private Dsp(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static Dsp Create(string name) =>
        new(Guid.NewGuid(), Guard.RequiredText(name, "DSP name", ValidationConstants.Dsp.NameMaxLength));

    public void Rename(string name) => Name = Guard.RequiredText(name, "DSP name", ValidationConstants.Dsp.NameMaxLength);
}
