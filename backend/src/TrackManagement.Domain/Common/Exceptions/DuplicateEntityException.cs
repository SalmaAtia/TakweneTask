namespace TrackManagement.Domain.Common.Exceptions;

/// <summary>A value that must be unique across an aggregate type is already taken.</summary>
public sealed class DuplicateEntityException : DomainException
{
    public string EntityName { get; }
    public string PropertyName { get; }
    public object Value { get; }

    public DuplicateEntityException(string entityName, string propertyName, object value)
        : base($"{entityName} with {propertyName} '{value}' already exists.")
    {
        EntityName = entityName;
        PropertyName = propertyName;
        Value = value;
    }
}
