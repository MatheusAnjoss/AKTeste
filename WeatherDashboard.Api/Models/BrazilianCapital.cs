namespace WeatherDashboard.Api.Models
{
    public class BrazilianCapital
    {
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public static List<BrazilianCapital> GetCapitals()
        {
            return new List<BrazilianCapital>
            {
                new() { City = "Rio Branco", State = "AC", Latitude = -9.9747, Longitude = -67.8243 },
                new() { City = "Maceió", State = "AL", Latitude = -9.6658, Longitude = -35.7353 },
                new() { City = "Macapá", State = "AP", Latitude = 0.0389, Longitude = -51.0664 },
                new() { City = "Manaus", State = "AM", Latitude = -3.1190, Longitude = -60.0217 },
                new() { City = "Salvador", State = "BA", Latitude = -12.9714, Longitude = -38.5014 },
                new() { City = "Fortaleza", State = "CE", Latitude = -3.7319, Longitude = -38.5267 },
                new() { City = "Brasília", State = "DF", Latitude = -15.8267, Longitude = -47.9218 },
                new() { City = "Vitória", State = "ES", Latitude = -20.3155, Longitude = -40.3128 },
                new() { City = "Goiânia", State = "GO", Latitude = -16.6869, Longitude = -49.2648 },
                new() { City = "São Luís", State = "MA", Latitude = -2.5387, Longitude = -44.2828 },
                new() { City = "Cuiabá", State = "MT", Latitude = -15.6014, Longitude = -56.0979 },
                new() { City = "Campo Grande", State = "MS", Latitude = -20.4697, Longitude = -54.6201 },
                new() { City = "Belo Horizonte", State = "MG", Latitude = -19.9191, Longitude = -43.9386 },
                new() { City = "Belém", State = "PA", Latitude = -1.4558, Longitude = -48.5044 },
                new() { City = "João Pessoa", State = "PB", Latitude = -7.1195, Longitude = -34.8450 },
                new() { City = "Curitiba", State = "PR", Latitude = -25.4244, Longitude = -49.2654 },
                new() { City = "Recife", State = "PE", Latitude = -8.0476, Longitude = -34.8770 },
                new() { City = "Teresina", State = "PI", Latitude = -5.0892, Longitude = -42.8019 },
                new() { City = "Rio de Janeiro", State = "RJ", Latitude = -22.9068, Longitude = -43.1729 },
                new() { City = "Natal", State = "RN", Latitude = -5.7945, Longitude = -35.2110 },
                new() { City = "Porto Alegre", State = "RS", Latitude = -30.0346, Longitude = -51.2177 },
                new() { City = "Porto Velho", State = "RO", Latitude = -8.7612, Longitude = -63.9023 },
                new() { City = "Boa Vista", State = "RR", Latitude = 2.8235, Longitude = -60.6758 },
                new() { City = "Florianópolis", State = "SC", Latitude = -27.5954, Longitude = -48.5480 },
                new() { City = "São Paulo", State = "SP", Latitude = -23.5505, Longitude = -46.6333 },
                new() { City = "Aracaju", State = "SE", Latitude = -10.9472, Longitude = -37.0731 },
                new() { City = "Palmas", State = "TO", Latitude = -10.1689, Longitude = -48.3317 }
            };
        }
    }
}
