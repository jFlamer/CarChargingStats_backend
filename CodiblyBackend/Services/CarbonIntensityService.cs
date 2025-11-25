using CodiblyBackend.Models;
using System.Text.Json;

namespace CodiblyBackend.Services
{
    public class CarbonIntensityService : InterfaceCarbonIntensityService
    {
        private readonly HttpClient _httpClient;

        // Wstrzykujemy HttpClient przez konstruktor
        public CarbonIntensityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<IntervalData>> GetCIDataAsync3Days()
        {
            // 1. Ustalamy zakres dat: od dzisiaj (północ) do za 3 dni
            // Używamy UTC, bo API operuje na czasie "Z" (Zulu time)
            var from = DateTime.UtcNow.Date.ToString("yyyy-MM-ddTHH:mmZ");
            var to = DateTime.UtcNow.Date.AddDays(3).ToString("yyyy-MM-ddTHH:mmZ");

            // 2. Budujemy URL zgodnie z dokumentacją: 
            // https://carbon-intensity.github.io/api-definitions/?shell#get-generation-from-to [cite: 15]
            var url = $"https://api.carbonintensity.org.uk/generation/{from}/{to}";

            // 3. Pobieramy dane
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                // Tutaj w prawdziwym projekcie warto rzucić własny wyjątek lub zalogować błąd
                return new List<IntervalData>();
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            // 4. Deserializujemy JSON na nasze obiekty
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<CarbonIntensityResponse>(jsonString, options);

            return result?.Data ?? new List<IntervalData>();
        }
    }
}