using Microsoft.EntityFrameworkCore;
using WeatherDashboard.Api.Models;

namespace WeatherDashboard.Api.Data
{
    public class WeatherContext : DbContext
    {
        public WeatherContext(DbContextOptions<WeatherContext> options) : base(options)
        {
        }

        public DbSet<WeatherData> WeatherData { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WeatherData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.State).IsRequired().HasMaxLength(2);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.Icon).HasMaxLength(10);
                
                entity.HasIndex(e => new { e.City, e.State, e.Timestamp });
            });
        }
    }
}
