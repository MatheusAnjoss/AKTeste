using System.Text.Json.Serialization;

namespace WeatherDashboard.Api.Models
{
    public class OneCallResponse
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }
        
        [JsonPropertyName("lon")]
        public double Lon { get; set; }
        
        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;
        
        [JsonPropertyName("current")]
        public CurrentWeather Current { get; set; } = new();
    }

    public class CurrentWeather
    {
        [JsonPropertyName("dt")]
        public long Dt { get; set; }
        
        [JsonPropertyName("temp")]
        public double Temp { get; set; }
        
        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }
        
        [JsonPropertyName("pressure")]
        public double Pressure { get; set; }
        
        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }
        
        [JsonPropertyName("uvi")]
        public double Uvi { get; set; }
        
        [JsonPropertyName("clouds")]
        public int Clouds { get; set; }
        
        [JsonPropertyName("wind_speed")]
        public double WindSpeed { get; set; }
        
        [JsonPropertyName("wind_deg")]
        public int WindDeg { get; set; }
        
        [JsonPropertyName("weather")]
        public WeatherInfo[] Weather { get; set; } = Array.Empty<WeatherInfo>();
    }

    public class WeatherInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("main")]
        public string Main { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;
    }
}
