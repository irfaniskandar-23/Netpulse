# Concept: Global Exception Handling in ASP.NET Core

> Phase 1 output — addresses all questions and assumptions from `developer-overview.md`

---

## 1. Plain language answer

Global exception handling is a single place in your application that intercepts any
unhandled exception and converts it into a clean, structured HTTP error response before
the client ever sees it.

---

## 2. Real-world analogy

Think of a **customer service desk at a department store**. No matter what goes wrong
internally — a register breaks, stock system is down, a product is genuinely out of stock —
customers never deal with the raw internal problem. They come to the customer service desk,
which translates the issue into one of two responses:

- **Expected problem** (out of stock) → specific, helpful message: *"Sorry, that item is unavailable"*
- **System failure** (register broken) → generic message: *"We're having technical difficulties, please try again"*

The customer never sees the internal error log. The desk handles everything. That desk
is your `IExceptionHandler`.

---

## 3. The problem it solves

**Without global exception handling**, every controller needs its own try/catch:

```csharp
// Every controller reinvents error handling — inconsistent shapes, leaking detail
[HttpGet("{id}")]
public async Task<IActionResult> GetOrder(int id)
{
    try
    {
        var order = await _orderService.GetAsync(id);
        return Ok(order);
    }
    catch (NotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = ex.Message }); // leaks exception detail
    }
}
```

Do this across 20 controllers and you get:

- **Inconsistent error shapes** — one controller returns `{ "message": "..." }`, another
  returns `{ "error": "..." }`, a third returns a plain string
- **Leaked stack traces** — one developer forgets to strip exception detail in production,
  sensitive internal structure visible to attackers
- **No central point** — you cannot add a trace ID or log enrichment to error responses
  in one place

**With `IExceptionHandler`**, the logic is written once. Every unhandled exception —
from controllers, from middleware, from anywhere — flows through it.

---

## 4. How it works

### Two types of exceptions — two different responses

**Domain exceptions** — you throw these yourself to signal expected business failures.
A resource was not found. A request failed validation. These are not bugs — they have
meaningful HTTP equivalents and descriptive messages are safe to send to the client.

**System exceptions** — unexpected failures. A null reference, a database connection
drop, a serialization error. These are bugs or infrastructure failures. The client
should never see the internal detail.

**Pipeline diagram:** [View in Excalidraw](https://excalidraw.com/#json=i4HT3PR9QGWjEvDazXdnD,ME4dF5L_0GaMmuk-Y8pW7Q)

### `IExceptionHandler` — the Microsoft-recommended interface

Introduced in .NET 8. You implement `TryHandleAsync`:

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // inspect exception type, write response
        // return true = handled, pipeline stops
        // return false = not handled, next registered handler gets a chance
    }
}
```

You can register multiple handlers — they are tried in registration order. The first
one that returns `true` wins.

### ProblemDetails — RFC 7807 standard

Your assumption is correct — ProblemDetails is the standard. It is defined by
[RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) and is what Microsoft's
own middleware produces. It gives HTTP APIs a consistent, machine-readable error format.

```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Order #123 was not found",
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "extensions": {
    "traceId": "abc-xyz-123"
  }
}
```

Your original assumption `{ "message": "internal server error" }` is a plain object —
not wrong, but not standard. ProblemDetails is preferable because clients and tooling
can parse it reliably.

### The response header you were thinking of

The trace ID. It lives in two places:

1. `ProblemDetails.extensions["traceId"]` — part of the response body
2. `X-Trace-Id` response header — so clients can read it without parsing the body

This is exactly why TraceContext is module 03 and runs inside ExceptionHandler. By the
time an exception bubbles up to `IExceptionHandler`, the trace ID is already on
`HttpContext.Items`, ready to be read and included in both places.

### Why your assumption about "clear messages" needs a boundary

You wrote: *"API need to convey the message clearly to end user — like database down,
missing configurations."*

The rule is:

| Exception type | What client sees | What logs see |
|---|---|---|
| Domain (e.g. not found) | Specific helpful message — `"Order #123 not found"` | Full exception |
| System (e.g. DB down) | Generic — `"An unexpected error occurred"` | Full exception + stack trace |

Telling a client *"database is down"* or *"missing configuration key"* leaks your
infrastructure topology. An attacker now knows your database is a separate server and
your app has a misconfiguration. The client only needs to know whether the failure was
their fault (domain) or yours (system).

---

## 5. Your new questions

### Result pattern vs throwing domain exceptions

You are right that this debate exists. The Result pattern means instead of throwing
`ResourceNotFoundException`, your service returns `Result<Order, Error>` — a value
representing either success or failure. The caller checks the result rather than
catching an exception.

**Argument for it:** exceptions are designed for truly unexpected failures. Using them
for expected business outcomes feels semantically wrong. Result pattern makes failure
a first-class part of the method signature — callers cannot ignore it.

**Argument against it:** it adds type complexity everywhere. Every method returns a
Result, every caller must unwrap it.

**For Netpulse:** we use domain exceptions — it is the conventional ASP.NET Core
approach and pairs naturally with `IExceptionHandler`. The Result pattern is worth
knowing about but is a separate architectural decision.

---

### Why exceptions are expensive

When you `throw`, the runtime does three costly things:

1. **Captures the full stack trace** — walks the entire call stack and records every
   frame. This is a heap allocation proportional to your call depth.
2. **Unwinds the stack** — walks back up through every method looking for a matching
   `catch` handler.
3. **JIT deoptimisation** — methods containing `try/catch` blocks can be harder for
   the JIT compiler to optimise.

For a system exception that happens once in a blue moon, this cost is irrelevant. For
a `NotFoundException` firing on every "check if this email exists" call in a
high-throughput API, it adds up. That is the real argument for the Result pattern in
hot paths — not correctness, just performance.

---

### CancellationToken in `TryHandleAsync`

The `ct` parameter is `HttpContext.RequestAborted` — it is cancelled when the client
disconnects or the request times out.

**Counterintuitive rule: do not pass `ct` to `WriteAsJsonAsync` inside the exception
handler. Use `CancellationToken.None` instead.**

Why: if the client already disconnected, `ct` is already cancelled. Passing it to
`WriteAsJsonAsync` causes that write to throw an `OperationCanceledException` — now
your exception handler is throwing an exception while handling an exception. The
framework will write nothing to the client (the connection is gone anyway), so using
`CancellationToken.None` is both safe and correct.

```csharp
// Wrong — if client disconnected, ct is cancelled, WriteAsJsonAsync throws
await context.Response.WriteAsJsonAsync(problem, cancellationToken);

// Right — write completes or silently fails, no secondary exception
await context.Response.WriteAsJsonAsync(problem, CancellationToken.None);
```

CancellationToken does not prevent memory leaks on its own — it signals that work
should stop. The caller must observe it. Disconnected TCP connections are surfaced
via `RequestAborted` cancellation, which is exactly this token.

---

### What else you need to know in global exception handling

1. **Multiple handlers** — you can register more than one `IExceptionHandler`. They
   are tried in registration order. Useful for separating domain exception mapping from
   system exception fallback. Each returns `true` if handled, `false` to pass to the next.

2. **Register `AddProblemDetails()`** — call `builder.Services.AddProblemDetails()` in
   `Program.cs` alongside your handler registration. This configures the built-in
   ProblemDetails serialisation support.

3. **Environment-specific detail** — expose stack traces only in Development:

   ```csharp
   Detail = env.IsDevelopment() ? exception.ToString() : "An unexpected error occurred"
   ```

4. **`OperationCanceledException` is not a system error** — if the client cancels the
   request (navigates away, timeout), ASP.NET Core throws `OperationCanceledException`.
   This is not a bug — it is expected. Handle it separately and return `true` without
   writing a response body (there is nobody to respond to).

---

## 6. Code example

**Wrong — try/catch in every controller, inconsistent shapes, leaking detail:**

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetOrder(int id)
{
    try
    {
        var order = await _service.GetAsync(id);
        return Ok(order);
    }
    catch (Exception ex)
    {
        // leaks exception message to client — could expose internal detail
        return StatusCode(500, new { message = ex.Message });
    }
}
```

**Right — throw and forget, let `IExceptionHandler` do the rest:**

```csharp
// Controller — just throws, never formats errors
[HttpGet("{id}")]
public async Task<IActionResult> GetOrder(int id)
{
    var order = await _service.GetAsync(id);
    return Ok(order);
    // ResourceNotFoundException thrown by service bubbles to GlobalExceptionHandler
}

// GlobalExceptionHandler — one place, consistent ProblemDetails shape
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(IHostEnvironment env) => _env = env;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Client cancelled — not an error, no response needed
        if (exception is OperationCanceledException)
            return true;

        var (status, title) = exception switch
        {
            ResourceNotFoundException => (404, "Not Found"),
            ValidationException       => (422, "Validation Failed"),
            _                         => (500, "Server Error")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title  = title,
            // System errors: generic message to client, real detail stays in logs
            Detail = status == 500 && !_env.IsDevelopment()
                ? "An unexpected error occurred"
                : exception.Message
        };

        context.Response.StatusCode = status;

        // CancellationToken.None — not ct — see concept notes above
        await context.Response.WriteAsJsonAsync(problem, CancellationToken.None);
        return true;
    }
}
```

---

## 7. Verify understanding

Your developer overview said:

> *"Create a generic error response wrapper and default to a normal message"*

That is correct for system exceptions but only half the story.

**Question:** A client sends `GET /orders/999` and order 999 does not exist in the
database. Which response is correct, and why are the other two wrong?

**A)** `{ status: 500, detail: "An unexpected error occurred" }`

**B)** `{ status: 404, detail: "Order 999 was not found" }`

**C)** `{ status: 404, detail: "System.NullReferenceException: Object reference not set..." }`

If you can explain why B is correct and articulate what is specifically wrong with A
and C, the concept has landed.

---

## Microsoft References

- [Handle errors in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling) — `IExceptionHandler`, `UseExceptionHandler`, development vs production behaviour
- [IExceptionHandler interface](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.diagnostics.iexceptionhandler) — `TryHandleAsync` signature and registration
- [Problem Details for HTTP APIs (RFC 7807)](https://datatracker.ietf.org/doc/html/rfc7807) — the standard ProblemDetails format
- [ProblemDetails class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails) — properties, extensions, usage in ASP.NET Core
- [CancellationToken and request cancellation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/request-timeouts) — request timeouts, `RequestAborted`, and cancellation handling
- [Performance best practices — exceptions](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging) — why exceptions are expensive and when to avoid them for control flow
