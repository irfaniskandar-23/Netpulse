# Checklist: Module 02 — Global Exception Handling

> Phase 2 output — implementation plan for GlobalExceptionHandler

---

## What we are building

A single `GlobalExceptionHandler` that intercepts every unhandled exception in the
application and converts it into a structured ProblemDetails response. Controllers
throw and forget — the handler does all the formatting.

---

## Why each piece exists

### `GlobalExceptionHandler : IExceptionHandler`
The Microsoft-recommended interface from .NET 8+. Implements `TryHandleAsync` which
receives the exception and writes the HTTP response. Registered as a service so DI
works correctly — no singleton lifetime trap.

### `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()`
Registers the handler with the DI container. Without this, `UseExceptionHandler()`
has nothing to call.

### `builder.Services.AddProblemDetails()`
Configures the built-in ProblemDetails serialisation support. Required alongside
`AddExceptionHandler` — without it, the response may not serialise correctly.

### `app.UseDeveloperExceptionPage()` vs `app.UseExceptionHandler()`
Environment split in `Program.cs` — not inside the handler:
- **Development** → `UseDeveloperExceptionPage()` — full stack trace, built-in
- **Production** → `UseExceptionHandler()` — our handler, clean ProblemDetails only

---

## Files

- [ ] `src/Netpulse.Api/Middleware/GlobalExceptionHandler.cs` — handler implementation
- [ ] `src/Netpulse.Api/Program.cs` — service registration + pipeline wiring

---

## Exception mapping

| Exception type | HTTP status | Title |
|---|---|---|
| `ResourceNotFoundException` | 404 | Not Found |
| `OperationCanceledException` | — (no response) | Client disconnected |
| Anything else | 500 | Server Error |

---

## Curl verification

```bash
# Happy path
curl http://localhost:5219/api/orders/1
# → 200 {"id":1,"item":"Sample Order","status":"Pending"}

# Domain exception → structured 404
curl http://localhost:5219/api/orders/999
# → 404 {"status":404,"title":"Not Found","detail":"Order with id '999' was not found."}

# System exception → structured 500
curl http://localhost:5219/api/orders/crash
# → 500 {"status":500,"title":"Server Error","detail":"Simulated system failure..."}
```
