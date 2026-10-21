namespace TrackManagement.Domain.Common.Exceptions;

/// <summary>An invalid value or an operation not allowed in the entity's current state.</summary>
public sealed class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }
}
