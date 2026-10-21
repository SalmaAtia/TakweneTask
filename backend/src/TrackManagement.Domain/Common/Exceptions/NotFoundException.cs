namespace TrackManagement.Domain.Common.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} '{key}' was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}
