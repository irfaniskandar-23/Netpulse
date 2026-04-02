using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Netpulse.Api.Exceptions;

namespace Netpulse.Api.Middleware;

/// <summary>
/// Intercepts every unhandled exception in the application and converts it into a
/// structured ProblemDetails HTTP response. Registered as the outermost middleware
/// so it acts as a safety net for the entire pipeline.
///
/// Two paths through this handler:
///   Domain exceptions  → specific HTTP status (404, 422, etc.) with a descriptive message
///   System exceptions  → HTTP 500 with the exception message (safe — dev sees developer page anyway)
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Client disconnected or request timed out — not an application error.
        // Return true to mark as handled but write no response (nobody is listening).
        if (exception is OperationCanceledException)
            return true;

        var (status, title) = exception switch
        {
            ResourceNotFoundException => (StatusCodes.Status404NotFound,            "Not Found"),
            _                         => (StatusCodes.Status500InternalServerError,  "Server Error")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title  = title,
            Detail = exception.Message
        };

        context.Response.StatusCode = status;

        // CancellationToken.None — not the request token. If the client disconnected,
        // the request token is already cancelled and passing it here would throw
        // OperationCanceledException inside the exception handler itself.
        await context.Response.WriteAsJsonAsync(problem, CancellationToken.None);

        return true;
    }
}
