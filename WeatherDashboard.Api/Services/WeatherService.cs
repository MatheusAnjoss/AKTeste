using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using WeatherDashboard.Api.Data;
using WeatherDashboard.Api.Models;
using System.Globalization;

namespace WeatherDashboard.Api.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly WeatherContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(IHttpClientFactory httpFactory, WeatherContext db, IConfiguration config, ILogger<WeatherService> logger)
        {
            _httpFactory = httpFactory;
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task<WeatherData?> GetCurrentWeatherAsync(string city, string state)
        {
            var (lat, lon, normalizedCity, normalizedState) = ResolveCoordinates(city, state);
            if (lat == null || lon == null)
            {
                _logger.LogWarning("Coordinates not found for {City}-{State}", city, state);
                return null;
            }

            var apiKey = _config.GetValue<string>("OpenWeatherMap:ApiKey");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("OpenWeather API key is missing in configuration.");
                return null;
            }

            var baseUrl = _config.GetValue<string>("OpenWeatherMap:BaseUrl")?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "https://api.openweathermap.org/data/2.5";
                _logger.LogWarning("OpenWeatherMap:BaseUrl not set. Using default {BaseUrl}", baseUrl);
            }

            try
            {
                var client = _httpFactory.CreateClient();
                var url =
                    $"{baseUrl}/weather?lat={lat.Value.ToString(CultureInfo.InvariantCulture)}" +
                    $"&lon={lon.Value.ToString(CultureInfo.InvariantCulture)}" +
                    $"&appid={apiKey}&units=metric&lang=pt_br";

                var response = await client.GetFromJsonAsync<WeatherResponse>(url);
                if (response == null || response.Main == null || response.Weather == null || response.Weather.Length == 0)
                {
                    _logger.LogWarning("Invalid response from OpenWeather for {City}-{State}", normalizedCity, normalizedState);
                    return null;
                }

                var ts = DateTimeOffset.FromUnixTimeSeconds(response.Dt).UtcDateTime;

                var entity = new WeatherData
                {
                    City = normalizedCity,
                    State = normalizedState,
                    Country = response.Sys?.Country ?? "BR",
                    Temperature = response.Main.Temp,
                    FeelsLike = response.Main.FeelsLike,
                    MinTemperature = response.Main.TempMin,
                    MaxTemperature = response.Main.TempMax,
                    Humidity = response.Main.Humidity,
                    Pressure = response.Main.Pressure,
                    WindSpeed = response.Wind?.Speed ?? 0,
                    WindDirection = response.Wind?.Deg ?? 0,
                    Cloudiness = response.Clouds?.All ?? 0,
                    WeatherMain = response.Weather[0].Main,
                    WeatherDescription = response.Weather[0].Description,
                    Description = $"{response.Weather[0].Main} - {response.Weather[0].Description}",
                    Icon = response.Weather[0].Icon,
                    Timestamp = ts,
                    CreatedAt = DateTime.UtcNow
                };

                _db.WeatherData.Add(entity);
                await _db.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather for {City}-{State}", city, state);
                return null;
            }
        }

        public async Task<WeatherData?> GetLatestWeatherAsync(string city, string state)
        {
            var (_, _, normCity, normState) = ResolveCoordinates(city, state);
            return await _db.WeatherData
                .AsNoTracking()
                .Where(w => w.City == normCity && w.State == normState)
                .OrderByDescending(w => w.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WeatherData>> GetHistoricalDataAsync(string city, string state, DateTime startDate, DateTime endDate)
        {
            var (_, _, normCity, normState) = ResolveCoordinates(city, state);
            return await _db.WeatherData
                .AsNoTracking()
                .Where(w => w.City == normCity && w.State == normState && w.Timestamp >= startDate && w.Timestamp <= endDate)
                .OrderBy(w => w.Timestamp)
                .ToListAsync();
        }

        public async Task<List<WeatherData>> GetHistoricalDataAsync(string city, DateTime startDate, DateTime endDate)
        {
            var normCity = Normalize(city);
            return await _db.WeatherData
                .AsNoTracking()
                .Where(w => w.City == normCity && w.Timestamp >= startDate && w.Timestamp <= endDate)
                .OrderBy(w => w.Timestamp)
                .ToListAsync();
        }

        public async Task<List<WeatherData>> GetHistoricalByCountryAsync(string city, string countryCode, DateTime startDate, DateTime endDate)
        {
            var normCity = Normalize(city);
            var normCountry = Normalize(countryCode);
            return await _db.WeatherData
                .AsNoTracking()
                .Where(w => w.City == normCity && w.Country.ToLower() == normCountry.ToLower() && w.Timestamp >= startDate && w.Timestamp <= endDate)
                .OrderBy(w => w.Timestamp)
                .ToListAsync();
        }

        private static string Normalize(string value) => (value ?? string.Empty).Trim();

        private static (double? lat, double? lon, string city, string state) ResolveCoordinates(string city, string state)
        {
            var normCity = Normalize(city);
            var normState = Normalize(state).ToUpperInvariant();

            var match = Models.BrazilianCapital.GetCapitals()
                .FirstOrDefault(c =>
                    string.Equals(c.City, normCity, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrWhiteSpace(normState) || string.Equals(c.State, normState, StringComparison.OrdinalIgnoreCase)));

            if (match == null)
            {
                match = Models.BrazilianCapital.GetCapitals()
                    .FirstOrDefault(c => string.Equals(c.City, normCity, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                    return (null, null, normCity, normState);
                normState = match.State;
            }

            return (match.Latitude, match.Longitude, match.City, match.State);
        }
    }
}
