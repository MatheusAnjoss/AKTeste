using FluentAssertions;
using WeatherDashboard.Api.Models;
using Xunit;

public class BrazilianCapitalTests
{
    [Fact]
    public void GetCapitals_Returns27()
    {
        var list = BrazilianCapital.GetCapitals();
        list.Should().NotBeNull();
        list.Should().HaveCount(27);
    }
}