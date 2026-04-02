using Netpulse.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Use Newtonsoft.Json instead of System.Text.Json

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// ---------------------------------------------------------------
// Middleware pipeline — built up module by module
// ---------------------------------------------------------------

// [Module 02] Exception handling — outermost so it catches failures from every layer inside.
// Dev uses the built-in developer page (full stack trace).
// All other environments use GlobalExceptionHandler (clean ProblemDetails).
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler();

// [Module 03] Trace context — generates a trace ID for each request
// app.UseMiddleware<TraceContextMiddleware>();

// [Module 04] Request/response logging — enriched with trace ID from module 03
// app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
