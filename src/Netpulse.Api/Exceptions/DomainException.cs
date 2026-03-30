namespace Netpulse.Api.Exceptions;

/// <summary>
/// Base class for all domain exceptions — exceptions you throw intentionally
/// to signal an expected business failure (not found, validation failed, etc.).
///
/// Inheriting from this base lets GlobalExceptionHandler (module 02) distinguish
/// between a domain failure (map to a specific HTTP status) and a system failure
/// (always map to 500 with a generic message).
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
