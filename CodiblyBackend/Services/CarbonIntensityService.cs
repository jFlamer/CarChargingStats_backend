using CodiblyBackend.Models;
using System.Text.Json;

namespace CodiblyBackend.Services
{
    public class CarbonIntensityService : InterfaceCarbonIntensityService
    {
        private readonly HttpClient _httpClient;

        public CarbonIntensityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<IntervalData>> GetCIDataAsync3Days()
        {
            var from = DateTime.UtcNow.Date.ToString("yyyy-MM-ddTHH:mmZ");
            var to = DateTime.UtcNow.Date.AddDays(4).ToString("yyyy-MM-ddTHH:mmZ");

            var url = $"https://api.carbonintensity.org.uk/generation/{from}/{to}";

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<IntervalData>();
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<CarbonIntensityResponse>(jsonString, options);

            return result?.Data ?? new List<IntervalData>();
        }
    }
}