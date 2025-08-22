using WeatherDashboard.Api.Models;

namespace WeatherDashboard.Api.Services
{
    public interface IWeatherService
    {
        Task<WeatherData?> GetCurrentWeatherAsync(string city, string state);
        Task<WeatherData?> GetLatestWeatherAsync(string city, string state);
        Task<List<WeatherData>> GetHistoricalDataAsync(string city, string state, DateTime startDate, DateTime endDate);
        Task<List<WeatherData>> GetHistoricalDataAsync(string city, DateTime startDate, DateTime endDate);
        Task<List<WeatherData>> GetHistoricalByCountryAsync(string city, string countryCode, DateTime startDate, DateTime endDate);
    }
}

