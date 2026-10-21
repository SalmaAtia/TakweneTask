namespace TrackManagement.Domain.Common.Exceptions;

/// <summary>Base type for every exception raised by the domain.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
