namespace ECommerce.Domain.Exceptions;

/// <summary>Thrown when an operation would violate a domain invariant (e.g. insufficient stock).</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.") { }
}
