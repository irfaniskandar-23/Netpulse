namespace Netpulse.Api.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist.
/// Maps to HTTP 404 Not Found in GlobalExceptionHandler (module 02).
///
/// Usage: throw new ResourceNotFoundException("Order", id);
/// </summary>
public class ResourceNotFoundException : DomainException
{
    public ResourceNotFoundException(string resource, object id)
        : base($"{resource} with id '{id}' was not found.") { }
}
