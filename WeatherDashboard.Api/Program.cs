using Microsoft.EntityFrameworkCore;
using WeatherDashboard.Api.Data;
using WeatherDashboard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WeatherContext>(o =>
{
    o.UseInMemoryDatabase("WeatherDashboardDb");
});

builder.Services.AddHttpClient("OpenWeather", client =>
{
    var baseUrl = configuration.GetValue<string>("OpenWeatherMap:BaseUrl") ?? "https://api.openweathermap.org/data/2.5";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<WeatherSeedService>();
builder.Services.AddHostedService<WeatherBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<WeatherSeedService>();
    await seeder.SeedInitialDataAsync();
}

app.Run();
