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
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
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
var otlpUri = new Uri(otlpEndpoint);

// --- 1. Logging Configuration ---
// Clear default providers and force JSON output to console.
// This allows Promtail to parse the logs and automatically extract the TraceId.
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
});

// --- 2. OpenTelemetry Tracing, Metrics, and Logging Configuration ---
// Setup distributed telemetry to map the entire request lifecycle and metrics, sending it to Alloy.
builder
    .Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("EleveRats.Api"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation(); // Traces incoming HTTP requests
        tracing.AddHttpClientInstrumentation(); // Traces outgoing HTTP requests (e.g., to n8n)
        tracing.AddNpgsql(); // Traces all PostgreSQL queries (command text, duration, errors)

        // Export traces to Grafana Alloy via OTLP gRPC endpoint
        tracing.AddOtlpExporter(opt =>
        {
            opt.Endpoint = otlpUri;
            if (!string.IsNullOrWhiteSpace(otlpHeaders))
            {
                opt.Headers = otlpHeaders;
            }
        });
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();

        // Export metrics to Grafana Alloy via OTLP gRPC endpoint
        metrics.AddOtlpExporter(opt =>
        {
            opt.Endpoint = otlpUri;
            if (!string.IsNullOrWhiteSpace(otlpHeaders))
            {
                opt.Headers = otlpHeaders;
            }
        });
    });

// Also forward ASP.NET Core ILogger logs to OpenTelemetry OTLP
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
    options.AddOtlpExporter(opt =>
    {
        opt.Endpoint = otlpUri;
        if (!string.IsNullOrWhiteSpace(otlpHeaders))
        {
            opt.Headers = otlpHeaders;
        }
    });
});

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// builder.Services.AddHostedService<AntiIdlenessService>();
WebApplication app = builder.Build();

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

        string docsButton = env.IsDevelopment()
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
