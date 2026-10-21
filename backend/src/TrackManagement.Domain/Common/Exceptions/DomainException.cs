namespace TrackManagement.Domain.Common.Exceptions;

/// <summary>A domain rule was violated (invalid value or operation not allowed in the current state).</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
