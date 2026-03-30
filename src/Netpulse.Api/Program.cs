var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Use Newtonsoft.Json instead of System.Text.Json

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// ---------------------------------------------------------------
// Middleware pipeline — built up module by module
// ---------------------------------------------------------------

// [Module 02] Exception handling — must be outermost to catch everything
// app.UseExceptionHandler();

// [Module 03] Trace context — generates a trace ID for each request
// app.UseMiddleware<TraceContextMiddleware>();

// [Module 04] Request/response logging — enriched with trace ID from module 03
// app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
