using Microsoft.AspNetCore.Mvc;
using Netpulse.Api.Exceptions;

namespace Netpulse.Api.Controllers;

/// <summary>
/// Demo controller used across all middleware modules.
///
/// Each endpoint is designed to trigger a specific scenario so you can observe
/// how the pipeline behaves before and after each module is added.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    // Fake data boundary — ids above this threshold simulate a "not found" domain scenario
    private const int MaxValidOrderId = 100;

    /// <summary>
    /// Returns a fake order for ids 1–100.
    /// Throws ResourceNotFoundException for any id above 100.
    ///
    /// Before module 02: returns a raw unhandled exception response (500).
    /// After module 02: returns a structured 404 ProblemDetails response.
    /// </summary>
    [HttpGet("{id:int}")]
    public IActionResult GetOrder(int id)
    {
        if (id > MaxValidOrderId)
            throw new ResourceNotFoundException("Order", id);

        return Ok(new { Id = id, Item = "Sample Order", Status = "Pending" });
    }

    /// <summary>
    /// Deliberately throws a system exception to simulate an unexpected failure.
    ///
    /// Before module 02: leaks raw exception detail to the client.
    /// After module 02: returns a generic 500 ProblemDetails response.
    /// </summary>
    [HttpGet("crash")]
    public IActionResult Crash()
    {
        throw new InvalidOperationException("Simulated system failure — something broke unexpectedly.");
    }
}
