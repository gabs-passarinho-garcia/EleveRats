// <copyright file="Program.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
// Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>
using EleveRats.Core;
using EleveRats.Modules.Users;
using Grafana.OpenTelemetry;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prometheus;
using Scalar.AspNetCore;

// using EleveRats.Services;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddUsersModule(builder.Configuration);

// --- 0. Infrastructure & Telemetry Parameters ---
// OTLP Endpoint and Headers for Grafana Alloy (Logs, Traces, Metrics).
// Can be overridden via environment variables:
// - OpenTelemetry__AlloyOtlpEndpoint
// - OpenTelemetry__AlloyOtlpHeaders
string otlpEndpoint =
    builder.Configuration["OpenTelemetry:AlloyOtlpEndpoint"] ?? Constants.AlloyOtlpEndpoint;
string? otlpHeaders = builder.Configuration["OpenTelemetry:AlloyOtlpHeaders"];

// Bridge custom environment variables to standard OpenTelemetry ones for the Grafana SDK
Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", otlpEndpoint);
if (!string.IsNullOrWhiteSpace(otlpHeaders))
{
    Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS", otlpHeaders);
}

// --- 1. Logging Configuration ---
// Clear default providers and force JSON output to console.
// This allows Promtail to parse the logs and automatically extract the TraceId.
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
});

// --- 2. OpenTelemetry Tracing, Metrics, and Logging Configuration (Grafana SDK) ---
// Setup distributed telemetry to map the entire request lifecycle and metrics, sending it to Alloy.
// The Grafana SDK automatically configures standard instrumentations and OTLP exporters.
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.UseGrafana();
});

builder
    .Services.AddOpenTelemetry()
    .UseGrafana()
    .WithTracing(tracing =>
        tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddNpgsql()
    )
    .WithMetrics(metrics =>
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter("Npgsql")
    );

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// builder.Services.AddHostedService<AntiIdlenessService>();
WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Captures HTTP metrics (request count, duration, status codes) for Prometheus
app.UseHttpMetrics();

app.MapGet(
    "/",
    async (IWebHostEnvironment env) =>
    {
        string dotnetVersion = System
            .Runtime
            .InteropServices
            .RuntimeInformation
            .FrameworkDescription;
        string filePath = Path.Combine(env.ContentRootPath, "wwwroot", "index.html");

        if (!System.IO.File.Exists(filePath))
        {
            return Results.NotFound("index.html not found");
        }

        string html = await System.IO.File.ReadAllTextAsync(filePath);
        html = html.Replace("{{dotnetVersion}}", dotnetVersion, StringComparison.Ordinal);

        string docsButton = (env.IsDevelopment() || env.IsEnvironment("Local"))
            ? @"<a href=""/scalar/v1"" class=""btn-docs"" target=""_blank"">
            <svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"">
                <path d=""M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z""></path>
                <path d=""M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z""></path>
            </svg>
            Documentação
        </a>"
            : string.Empty;

        html = html.Replace("{{docsButton}}", docsButton, StringComparison.Ordinal);

        return Results.Content(html, "text/html");
    }
);

// Exposes the /metrics endpoint for Prometheus scraping
app.MapMetrics();

// Health Check endpoint for Docker Compose and orchestrators
app.MapHealthChecks("/health");

await app.RunAsync();
