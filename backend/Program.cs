using Prometheus;
using Scalar.AspNetCore;
using EleveRats.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Logging Configuration ---
// Clear default providers and force JSON output to console.
// This allows Promtail to parse the logs and automatically extract the TraceId.
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
});

// --- 2. OpenTelemetry Tracing Configuration ---
// Setup distributed tracing to map the entire request lifecycle and send it to Tempo.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("EleveRats.Api"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation(); // Traces incoming HTTP requests
        tracing.AddHttpClientInstrumentation(); // Traces outgoing HTTP requests (e.g., to n8n)
        
        // Export traces to Grafana Tempo via OTLP gRPC endpoint
        tracing.AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri("http://tempo:4317");
        });
    });

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<AntiIdlenessService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Captures HTTP metrics (request count, duration, status codes) for Prometheus
app.UseHttpMetrics();

app.MapGet("/", async (IWebHostEnvironment env) => 
{
    var dotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
    var filePath = Path.Combine(env.ContentRootPath, "wwwroot", "index.html");
    
    if (!System.IO.File.Exists(filePath)) 
    {
        return Results.NotFound("index.html not found");
    }
    
    var html = await System.IO.File.ReadAllTextAsync(filePath);
    html = html.Replace("{{dotnetVersion}}", dotnetVersion);
    
    var docsButton = env.IsDevelopment() 
        ? @"<a href=""/scalar/v1"" class=""btn-docs"" target=""_blank"">
            <svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"">
                <path d=""M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z""></path>
                <path d=""M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z""></path>
            </svg>
            Documentação
        </a>"
        : string.Empty;
        
    html = html.Replace("{{docsButton}}", docsButton);
    
    return Results.Content(html, "text/html");
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Exposes the /metrics endpoint for Prometheus scraping
app.MapMetrics();

// Health Check endpoint for Docker Compose and orchestrators
app.MapHealthChecks("/health");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}