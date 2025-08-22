using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using WeatherDashboard.Api.Controllers;
using WeatherDashboard.Api.Models;
using WeatherDashboard.Api.Services;
using Xunit;

public class WeatherControllerTests
{
    [Fact]
    public async Task GetChartData_FallbacksToCurrent_WhenNoHistory()
    {
        var svc = new Mock<IWeatherService>();
        var city = "Florianópolis";
        var state = "SC";
        var start = new DateTime(2025, 8, 20);
        var end   = new DateTime(2025, 8, 21);

        svc.Setup(s => s.GetHistoricalDataAsync(city, state, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
           .ReturnsAsync(new List<WeatherData>());

        var now = DateTime.UtcNow;
        svc.Setup(s => s.GetCurrentWeatherAsync(city, state))
           .ReturnsAsync(new WeatherData { City = city, State = state, Temperature = 22, Humidity = 60, Timestamp = now });

        var cfg = new ConfigurationBuilder().Build();
        var ctrl = new WeatherController(svc.Object, cfg);

        var res = await ctrl.GetChartData(city, state, start, end);
        var ok = res as OkObjectResult;
        ok.Should().NotBeNull();

        dynamic body = ok!.Value!;
        ((IEnumerable<string>)body.labels).Should().HaveCount(1);
        ((IEnumerable<object>)body.temperatureData).Should().HaveCount(1);
        ((IEnumerable<object>)body.humidityData).Should().HaveCount(1);

        svc.Verify(s => s.GetHistoricalDataAsync(city, state, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        svc.Verify(s => s.GetCurrentWeatherAsync(city, state), Times.Once);
    }

    [Fact]
    public async Task GetChartData_ReturnsBadRequest_WhenDatesInvalid()
    {
        var svc = new Mock<IWeatherService>();
        var cfg = new ConfigurationBuilder().Build();
        var ctrl = new WeatherController(svc.Object, cfg);

        var res = await ctrl.GetChartData("Cidade", "ST", default, default);
        res.Should().BeOfType<BadRequestObjectResult>();
    }
}