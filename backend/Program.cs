using Prometheus;
using Scalar.AspNetCore;
using EleveRats.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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

// Captura métricas HTTP (quantidade de requests, duração, status codes)
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

// Expõe o endpoint /metrics para o Prometheus coletar os dados
app.MapMetrics();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
