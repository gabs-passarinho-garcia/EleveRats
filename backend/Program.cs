var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
