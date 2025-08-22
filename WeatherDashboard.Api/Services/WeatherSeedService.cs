using Microsoft.EntityFrameworkCore;
using WeatherDashboard.Api.Data;
using WeatherDashboard.Api.Models;

namespace WeatherDashboard.Api.Services
{
    public class WeatherSeedService
    {
        private readonly WeatherContext _db;
        private readonly IWeatherService _weatherService;
        private readonly IConfiguration _config;
        private readonly ILogger<WeatherSeedService> _logger;

        public WeatherSeedService(WeatherContext db, IWeatherService weatherService, IConfiguration config, ILogger<WeatherSeedService> logger)
        {
            _db = db;
            _weatherService = weatherService;
            _config = config;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            await _db.Database.EnsureCreatedAsync();

            if (!await _db.WeatherData.AnyAsync())
            {
                var defaultCity = _config.GetValue<string>("WeatherSettings:DefaultCity") ?? "São Paulo";
                var capital = Models.BrazilianCapital.GetCapitals()
                    .FirstOrDefault(c => string.Equals(c.City, defaultCity, StringComparison.OrdinalIgnoreCase));
                var state = capital?.State ?? "SP";

                _logger.LogInformation("Seeding first weather sample for {City}-{State}", defaultCity, state);
                await _weatherService.GetCurrentWeatherAsync(defaultCity, state);
            }
        }

        public async Task SeedInitialDataAsync()
        {
            if (await _db.WeatherData.AnyAsync())
            {
                _logger.LogInformation("Weather data already exists, skipping seed");
                return;
            }

            _logger.LogInformation("Seeding initial weather data for all capitals");
            foreach (var c in BrazilianCapital.GetCapitals())
            {
                try
                {
                    await _weatherService.GetCurrentWeatherAsync(c.City, c.State);
                    await Task.Delay(400);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Seed failed for {City}-{State}", c.City, c.State);
                }
            }
        }

        private string GetRandomWeatherMain(Random random)
        {
            var mains = new[] { "Clear", "Clouds", "Rain", "Drizzle", "Thunderstorm", "Snow", "Mist" };
            return mains[random.Next(mains.Length)];
        }

        private string GetRandomDescription(Random random)
        {
            var descriptions = new[] { "céu limpo", "poucas nuvens", "nuvens dispersas", "nuvens quebradas", "chuva leve", "sol" };
            return descriptions[random.Next(descriptions.Length)];
        }

        private string GetRandomIcon(Random random)
        {
            var icons = new[] { "01d", "02d", "03d", "04d", "09d", "10d", "11d", "13d", "50d" };
            return icons[random.Next(icons.Length)];
        }
    }
}

