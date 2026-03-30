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
using EleveRats.Core.Application.Interfaces;
using EleveRats.Core.Infra.Caching;
using EleveRats.Modules.Users;
using Grafana.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prometheus;
using Scalar.AspNetCore;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// --- 0. Infrastructure & Telemetry Parameters ---
// OTLP Endpoint and Headers for Grafana Alloy (Logs, Traces, Metrics).
string otlpEndpoint =
    builder.Configuration["OpenTelemetry:AlloyOtlpEndpoint"] ?? Constants.AlloyOtlpEndpoint;
string? otlpHeaders = builder.Configuration["OpenTelemetry:AlloyOtlpHeaders"];

// Redis Connection String with URI Support (Upstash, Railway, etc.)
string rawRedisConnectionString =
    builder.Configuration["Cache:Redis:ConnectionString"] ?? Constants.DefaultRedisConnectionString;
string redisConnectionString = RedisConfigurationHelper.GetConnectionString(
    rawRedisConnectionString
);

// Bridge custom environment variables to standard OpenTelemetry ones for the Grafana SDK
Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", otlpEndpoint);
if (!string.IsNullOrWhiteSpace(otlpHeaders))
{
    Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS", otlpHeaders);
}

// --- 1. Infrastructure Services (Cross-Cutting) ---

// 🏀 JOGADA DE MESTRE: Instanciar o Multiplexer uma única vez para reaproveitamento total!
IConnectionMultiplexer redisConnection = await ConnectionMultiplexer.ConnectAsync(
    redisConnectionString
);

// Opcional, mas muito útil: Registrar o multiplexer no DI caso você precise dele cru em algum serviço
builder.Services.AddSingleton(redisConnection);

// Register distributed cache with Redis (Usando a conexão compartilhada)
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Usamos a factory para injetar a instância que já abrimos, evitando conexões duplicadas
    options.ConnectionMultiplexerFactory = () => Task.FromResult(redisConnection);
    options.InstanceName = "EleveRats:";
});

// Register OpenTelemetry with Grafana SDK e plugar o Redis
builder
    .Services.AddOpenTelemetry()
    .UseGrafana()
    .WithTracing(tracerBuilder => tracerBuilder.AddRedisInstrumentation(redisConnection));

// Register Core Cache Service
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// --- 2. Module Registration ---
// Users Module (Persistence, Repositories, Use Cases)
builder.Services.AddUsersModule(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();

// Health Checks
builder.Services.AddHealthChecks();

// OpenApi / Swagger
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Prometheus Metrics Endpoint
app.UseMetricServer();
app.UseHttpMetrics();

// Map Module Endpoints
// Each module should expose its own mapping extensions

// Root Endpoint - Dashboard / Status
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

        string docsButton =
            (env.IsDevelopment() || env.IsEnvironment("Local"))
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

// Map health checks
app.MapHealthChecks("/health");

await app.RunAsync();
