# Concept: ASP.NET Core Request Pipeline

> Phase 1 output — addresses all questions from `developer-overview.md` and teaches the complete concept

---

## 1. Plain language answer

The ASP.NET Core request pipeline is an ordered chain of components that every HTTP request
passes through — going in and out — before and after your controller runs.

---

## 2. Real-world analogy

Think of an **airport security checkpoint**. Every passenger (request) must pass through
the same lanes in the same fixed order — check-in, passport control, security scan, boarding
gate. Each lane can:

- **Let you through** — pass to the next lane
- **Turn you back early** — short-circuit (e.g. invalid passport → stopped right there, never reaches the gate)
- **Do something on the way in AND out** — security scans you going in, customs checks you coming out

Your controller is the flight itself. The middleware lanes run before and after it.

---

## 3. The problem it solves

**Without a pipeline**, every controller handles its own logging, authentication, and exception
catching. A 20-controller API means 20 places to forget error handling, 20 places where auth
can be inconsistently enforced, 20 places to duplicate the same code.

**With a pipeline**, you write the concern once, register it in `Program.cs`, and it applies
to every single request automatically — no controller needs to change.

**What goes wrong without it:**

- A new developer adds a controller and forgets try/catch — unhandled exceptions leak stack
  traces to the client in production
- Logging is inconsistent — some endpoints log, others don't, making incidents untraceable
- Auth is bypassed — one controller skips the auth check, exposing a sensitive endpoint

---

## 4. How it works

Think of middleware as a **Russian nesting doll**. Each middleware wraps the next one.
When a request arrives it unwraps from the outside in. When the response returns, it
re-wraps from the inside out.

```
Request →
  [Middleware A — in]
    [Middleware B — in]
      [Middleware C — in]
        [Controller Action]
      [Middleware C — out]
    [Middleware B — out]
  [Middleware A — out]
← Response
```

Each middleware calls `await next(context)` to hand control to the next component.
Code **before** `next` runs on the way **in** (request).
Code **after** `next` runs on the way **out** (response).

### The correct middleware order for Netpulse

```
[ExceptionHandler]       ← outermost — catches everything thrown inside
  [HttpsRedirection]     ← redirects HTTP → HTTPS before any processing
    [RequestLogging]     ← logs request on the way in, response on the way out
      [Authentication]   ← establishes who the caller is
        [Authorization]  ← checks what the caller is allowed to do
          [Controller]
```

**Pipeline diagram:** [View in Excalidraw](https://excalidraw.com/#json=3PZ4EELcvY4dxSJdY6S6R,BPER-6lcJ949SZaJQOYilA)

**Why this order matters:**

- ExceptionHandler must be outermost so it can catch exceptions from every layer inside it
- HttpsRedirection sits early so insecure requests are rejected before any business logic runs
- RequestLogging wraps the controller so it can capture both the request and the final response status
- Authentication must run before Authorization — you can't check permissions before establishing identity

---

## 5. Complete concept — what you need to know beyond your questions

### Two ways to write middleware

Microsoft provides two approaches. Always prefer `IMiddleware` — it is the recommended
pattern because it integrates with dependency injection properly.

**Convention-based (older pattern):**

```csharp
// Works but harder to test — DI is manual
public class MyMiddleware
{
    private readonly RequestDelegate _next;

    public MyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // before
        await _next(context);
        // after
    }
}
```

**`IMiddleware` (recommended pattern):**

```csharp
// Cleaner — DI injected via constructor, registered as a service
public class MyMiddleware : IMiddleware
{
    private readonly ILogger<MyMiddleware> _logger;

    public MyMiddleware(ILogger<MyMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // before
        await next(context);
        // after
    }
}

// Register in Program.cs
builder.Services.AddTransient<MyMiddleware>();
app.UseMiddleware<MyMiddleware>();
```

---

### `Use` vs `Run` vs `Map` — the three pipeline building blocks

| Method | Behaviour | When to use |
|---|---|---|
| `Use` | Calls the next middleware — pipeline continues | Most middleware |
| `Run` | **Terminates** the pipeline — never calls next | Terminal handlers (health check, catch-all) |
| `Map` | **Branches** the pipeline for a specific path | Path-specific middleware (e.g. `/admin/*`) |

**`Run` is a common mistake.** If you accidentally use `Run` instead of `Use`, everything
registered after it is silently skipped. No error, no warning — requests just stop there.

```csharp
// Wrong — Run terminates, UseAuthorization never runs
app.Run(async context => await context.Response.WriteAsync("Hello"));
app.UseAuthorization(); // ← never reached

// Right — Use passes control forward
app.Use(async (context, next) =>
{
    // do something
    await next(context); // ← passes to next middleware
});
app.UseAuthorization(); // ← this runs
```

---

### `UseWhen` and `MapWhen` — conditional branching

Apply middleware only to specific requests without splitting into separate apps:

```csharp
// Only run rate limiting for API routes
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseRateLimiter()
);
```

`UseWhen` rejoins the main pipeline after the branch.
`MapWhen` does not — it creates a completely separate branch.

---

### `RequestDelegate` — what it actually is

Every `next` parameter in middleware is a `RequestDelegate`:

```csharp
public delegate Task RequestDelegate(HttpContext context);
```

It is simply a function that takes an `HttpContext` and returns a `Task`. When you call
`await next(context)`, you are invoking the next middleware's `InvokeAsync` method.
The entire pipeline is a chain of these delegates — each one holding a reference to the next.

Understanding this demystifies the pattern: there is no framework magic, just nested async
function calls.

---

### Middleware vs Filters vs Minimal API — knowing when to use which

Developers often confuse these three. They run at different points in the request lifecycle:

| | Middleware | Filters | Minimal API route handler |
|---|---|---|---|
| Runs at | Pipeline level — every request | Controller/action level | Per endpoint |
| Has access to | `HttpContext` only | Controller context, model binding results | Route parameters, services |
| Use for | Logging, auth, tracing, exceptions | Validation, response formatting, auth policies on specific actions | Endpoint-specific logic |
| Registered in | `Program.cs` via `Use*` | `[ServiceFilter]`, `AddControllers()` filters | Inline lambda or handler method |

**Rule of thumb:** If it applies to every request → middleware. If it applies to specific
controllers or actions → filter. If it is the endpoint logic itself → route handler.

---

### Short-circuiting — deliberate vs accidental

**Deliberate short-circuit** — a middleware decides the request should not continue and
writes a response directly without calling `next`:

```csharp
// Deliberate — return 401 if no token present
if (!context.Request.Headers.ContainsKey("Authorization"))
{
    context.Response.StatusCode = 401;
    return; // ← does NOT call next
}
await next(context);
```

**Accidental short-circuit** — forgetting to call `next`:

```csharp
// Accidental — middleware silently swallows the request
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    var traceId = Guid.NewGuid();
    context.Items["TraceId"] = traceId;
    // forgot to call next — controller never runs, client gets empty 200
}
```

This is one of the most common middleware bugs. Always double-check that `next` is called
unless you are intentionally short-circuiting.

---

## 6. Addressing your specific questions

### Q: Confused on wiring up middleware in `Program.cs` — what is the extension method pattern?

The `Use*` methods in `Program.cs` are extension methods that register middleware in order.
The order you call them **is** the pipeline order. There is no magic — the first `Use*` call
is the outermost middleware.

```csharp
// Program.cs — the order here IS the pipeline order
app.UseExceptionHandler();    // 1st = outermost
app.UseHttpsRedirection();    // 2nd
app.UseSerilogRequestLogging(); // 3rd
app.UseAuthentication();      // 4th
app.UseAuthorization();       // 5th
app.MapControllers();         // innermost — the actual endpoints
```

Some teams wrap groups of registrations into extension methods for readability:

```csharp
// Extension method approach — same result, cleaner Program.cs
app.UseNetpulseMiddleware();

// Defined elsewhere:
public static IApplicationBuilder UseNetpulseMiddleware(this IApplicationBuilder app)
{
    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    return app;
}
```

Both approaches produce identical pipelines. The extension method is just organisation.

---

### Q: Does request/response logging middleware come before or after exception middleware?

**Exception middleware comes first (outermost). Logging comes second (inside it).**

Here is what happens when a controller throws an unhandled exception:

1. Exception bubbles up out of the controller
2. Logging middleware's `after next` code runs — Serilog captures the exception in the log entry
3. Exception bubbles up to ExceptionHandler middleware
4. ExceptionHandler formats a `ProblemDetails` response and writes it to the client

If you put logging *outside* exception middleware, logging would see the raw unhandled
exception before it is formatted — you'd log a 500 with no structured details. The current
order lets both do their job cleanly.

---

### Q: What is the difference between exception handler page in development vs production?

| Environment | What the client sees | Why |
|---|---|---|
| Development | Full exception detail — stack trace, message, source | Helps you debug locally |
| Production | Generic `ProblemDetails` response — no stack trace | Prevents leaking implementation details to attackers |

In Netpulse we will implement a `GlobalExceptionHandler` using `IExceptionHandler`
(the Microsoft-recommended interface from .NET 8+). It maps domain exceptions to HTTP
status codes and hides sensitive detail in production.

---

### Q: `UseAuthentication` and `UseAuthorization` are in the default template — can I add my own?

Yes. These two built-ins do specific jobs:

- `UseAuthentication` — reads the incoming token/cookie and populates `HttpContext.User` (who are you?)
- `UseAuthorization` — checks `HttpContext.User` against `[Authorize]` attributes (are you allowed?)

You **do not replace** these. You **compose around** them. For example:

```csharp
app.UseAuthentication();       // built-in — establish identity
app.UseMyTenantMiddleware();   // custom — extract tenant from claims
app.UseAuthorization();        // built-in — check permissions
```

In Netpulse we will add custom middleware for tracing and correlation IDs alongside the
built-in authentication middleware — not replacing it.

---

### Q: What is `UseHttpsRedirection`?

A built-in middleware that intercepts any plain HTTP request and returns a `301 Moved
Permanently` redirect to the HTTPS equivalent. It sits near the top of the pipeline so
insecure requests are turned away before any processing (logging, auth, controller) happens.

In local development with Kestrel you will typically see port 5000 (HTTP) redirected to
5001 (HTTPS). In production this is usually handled by the reverse proxy (IIS, nginx) before
the request even reaches your app.

---

### Q: What should and should not be done with `HttpContext`?

| ✅ Safe | ❌ Avoid |
|---|---|
| Read `HttpContext.Request` headers, path, query string | Storing `HttpContext` in a field or static variable |
| Write `HttpContext.Response` headers and status code | Accessing `HttpContext` from a background thread or after the request ends |
| Store per-request data in `HttpContext.Items` | Passing `HttpContext` into a singleton service |
| Access `HttpContext.User` for identity claims | |

**Why the ❌ items are dangerous:** `HttpContext` is created per-request and disposed when
the request ends. If you store it in a singleton service or access it from a background thread,
you risk reading data from a different request (data leak) or a null reference exception
(object already disposed).

Microsoft's recommended pattern is to capture only what you need from the context at the start
of the middleware, not to pass the context object itself downstream.

---

## 6. Code example

**Wrong — registering middleware in the wrong order:**

```csharp
// Wrong — logging is outermost, exceptions from it are uncaught
app.UseSerilogRequestLogging();
app.UseExceptionHandler();      // too late — won't catch logging exceptions
app.UseAuthentication();
app.MapControllers();
```

**Right — exception handler wraps everything:**

```csharp
// Right — exception handler is outermost, catches from all inner middleware
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

## 7. Verify understanding

**Question to answer before moving to Phase 2:**

> If a request comes in without an `Authorization` header, at which middleware does it
> short-circuit and what HTTP status code is returned? Walk through the pipeline order
> above and explain exactly where and why the request stops.

If you can answer this correctly, the pipeline order has landed.

Does that land? Ready to see the plan?

---

## Microsoft References

- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/) — official overview of the pipeline, `Use`, `Run`, `Map`, and ordering
- [Write custom ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write) — `IMiddleware` vs convention-based, DI integration
- [ASP.NET Core Built-in middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/#built-in-middleware) — full table of built-in middleware with recommended order
- [HttpContext in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/use-http-context) — safe and unsafe usage patterns
- [Handle errors in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling) — exception handler middleware, development vs production behaviour
- [Authentication and Authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/) — `UseAuthentication` vs `UseAuthorization` and how they compose
