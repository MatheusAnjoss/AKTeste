using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Api.Models;
using WeatherDashboard.Api.Services;

namespace WeatherDashboard.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IConfiguration _configuration;

        public WeatherController(IWeatherService weatherService, IConfiguration configuration)
        {
            _weatherService = weatherService;
            _configuration = configuration;
        }

        [HttpGet("capitals")]
        public ActionResult<List<BrazilianCapital>> GetCapitals()
        {
            return Ok(BrazilianCapital.GetCapitals());
        }

        [HttpGet("current/{city}/{state}")]
        public async Task<ActionResult<WeatherData>> GetCurrentWeather(string city, string state)
        {
            var weather = await _weatherService.GetCurrentWeatherAsync(city, state);
            if (weather == null)
                return NotFound($"Weather data not found for city: {city}");

            return Ok(weather);
        }

        [HttpGet("latest/{city}/{state}")]
        public async Task<ActionResult<WeatherData>> GetLatestWeather(string city, string state)
        {
            var weather = await _weatherService.GetLatestWeatherAsync(city, state);
            if (weather == null)
                return NotFound();

            return Ok(weather);
        }

        [HttpGet("historical/{city}/{state}")]
        public async Task<ActionResult<List<WeatherData>>> GetHistoricalWeather(
            string city, string state,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate == default || endDate == default)
                return BadRequest("Start date and end date are required");

            if (startDate > endDate)
                return BadRequest("Start date must be before end date");

            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            var endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

            var data = await _weatherService.GetHistoricalDataAsync(city, state, startUtc, endUtc);
            return Ok(data);
        }

        [HttpGet("statistics/{city}/{state}")]
        public async Task<ActionResult> GetDailyStatistics(string city, string state, [FromQuery] DateTime? date = null)
        {
            var targetLocal = (date ?? DateTime.Today).Date;
            var startUtc = DateTime.SpecifyKind(targetLocal, DateTimeKind.Local).ToUniversalTime();
            var endUtc = DateTime.SpecifyKind(targetLocal.AddDays(1).AddTicks(-1), DateTimeKind.Local).ToUniversalTime();

            var data = await _weatherService.GetHistoricalDataAsync(city, state, startUtc, endUtc);

            if (!data.Any())
                return NotFound();

            var statistics = new
            {
                Date = targetLocal.ToString("yyyy-MM-dd"),
                City = city,
                State = state,
                MinTemperature = data.Min(d => d.MinTemperature),
                MaxTemperature = data.Max(d => d.MaxTemperature),
                AverageTemperature = Math.Round(data.Average(d => d.Temperature), 2),
                AverageHumidity = Math.Round(data.Average(d => d.Humidity), 2),
                AveragePressure = Math.Round(data.Average(d => d.Pressure), 2),
                AverageWindSpeed = Math.Round(data.Average(d => d.WindSpeed), 2),
                DataPoints = data.Count
            };

            return Ok(statistics);
        }

        [HttpGet("chart-data/{city}/{state?}")]
        public async Task<IActionResult> GetChartData(
            string city,
            string? state,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate == default || endDate == default)
                return BadRequest("startDate and endDate are required");
            if (startDate > endDate)
                return BadRequest("startDate must be before endDate");

            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            var endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

            List<WeatherData> rows = string.IsNullOrWhiteSpace(state)
                ? await _weatherService.GetHistoricalDataAsync(city, startUtc, endUtc)
                : await _weatherService.GetHistoricalDataAsync(city, state, startUtc, endUtc);

            if (rows == null || rows.Count == 0)
            {
                state ??= BrazilianCapital.GetCapitals()
                    .FirstOrDefault(c => string.Equals(c.City, city, StringComparison.InvariantCultureIgnoreCase))?.State;

                var current = await _weatherService.GetCurrentWeatherAsync(city, state ?? "");
                if (current != null)
                    rows = new List<WeatherData> { current };
                else
                    rows = new List<WeatherData>();
            }

            var labels = rows.Select(r => r.Timestamp.ToLocalTime().ToString("s")).ToArray();
            var temperatureData = rows.Select(r => new
            {
                temperature = r.Temperature,
                minTemp = r.MinTemperature,
                maxTemp = r.MaxTemperature
            }).ToArray();
            var humidityData = rows.Select(r => new
            {
                humidity = r.Humidity
            }).ToArray();

            return Ok(new { labels, temperatureData, humidityData });
        }

        public async Task<IActionResult> GetHistorical(
            [FromQuery] string city,
            [FromQuery] string countryCode,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime? endDate
        )
        {
            if (startDate == default)
                return BadRequest("Start date is required");

            var toDate = endDate ?? startDate;

            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            var endUtc = DateTime.SpecifyKind(toDate, DateTimeKind.Local).ToUniversalTime();

            var data = await _weatherService.GetHistoricalByCountryAsync(city, countryCode, startUtc, endUtc);

            if (!data.Any())
                return NotFound();

            return Ok(data);
        }
    }
}
