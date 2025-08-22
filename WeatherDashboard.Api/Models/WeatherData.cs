using System.ComponentModel.DataAnnotations;

namespace WeatherDashboard.Api.Models
{
    public class WeatherData
    {
        public int Id { get; set; }
        
        [Required]
        public string City { get; set; } = string.Empty;
        
        [Required]
        public string State { get; set; } = string.Empty;
        
        [Required]
        public string Country { get; set; } = string.Empty;
        
        public double Temperature { get; set; }
        
        public double FeelsLike { get; set; }
        
        public double MinTemperature { get; set; }
        
        public double MaxTemperature { get; set; }
        
        public int Humidity { get; set; }
        
        public double Pressure { get; set; }
        
        public double WindSpeed { get; set; }
        
        public int WindDirection { get; set; }
        
        public int Cloudiness { get; set; }
        
        public string WeatherMain { get; set; } = string.Empty;
        
        public string WeatherDescription { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Icon { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
