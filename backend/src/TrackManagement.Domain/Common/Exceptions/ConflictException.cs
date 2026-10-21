namespace TrackManagement.Domain.Common.Exceptions;

/// <summary>The operation conflicts with existing state, e.g. a value that must be unique is already taken.</summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }
}
