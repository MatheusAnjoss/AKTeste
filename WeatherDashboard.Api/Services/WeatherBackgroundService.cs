using WeatherDashboard.Api.Models;

namespace WeatherDashboard.Api.Services
{
    public class WeatherBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<WeatherBackgroundService> _logger;
        private readonly TimeSpan _interval;
        private readonly string _defaultCity;

        public WeatherBackgroundService(IServiceProvider services, IConfiguration config, ILogger<WeatherBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
            var minutes = config.GetValue<int?>("WeatherSettings:UpdateIntervalMinutes") ?? 15;
            _interval = TimeSpan.FromMinutes(minutes <= 0 ? 15 : minutes);
            _defaultCity = config.GetValue<string>("WeatherSettings:DefaultCity") ?? "São Paulo";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var capitals = BrazilianCapital.GetCapitals();

            _logger.LogInformation("Weather background updater started. Interval: {Interval}", _interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();
                var weather = scope.ServiceProvider.GetRequiredService<IWeatherService>();

                foreach (var c in capitals)
                {
                    if (stoppingToken.IsCancellationRequested) break;
                    try
                    {
                        _logger.LogInformation("Background fetch for {City}-{State}", c.City, c.State);
                        await weather.GetCurrentWeatherAsync(c.City, c.State);
                        await Task.Delay(500, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Background fetch failed for {City}-{State}", c.City, c.State);
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task UpdateDefaultCityAsync(CancellationToken ct)
        {
            using var scope = _services.CreateScope();
            var weather = scope.ServiceProvider.GetRequiredService<IWeatherService>();

            var capital = Models.BrazilianCapital.GetCapitals()
                .FirstOrDefault(c => string.Equals(c.City, _defaultCity, StringComparison.OrdinalIgnoreCase));

            var state = capital?.State ?? "SP";

            _logger.LogInformation("Background fetch for {City}-{State}", _defaultCity, state);
            await weather.GetCurrentWeatherAsync(_defaultCity, state);
        }
    }
}
