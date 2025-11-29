using System.Text.Json.Serialization;

namespace CodiblyBackend.Models
{
    public class CarbonIntensityResponse
    {
        [JsonPropertyName("data")]
        public List<IntervalData> Data { get; set; } = new();
    }

    public class IntervalData
    {
        [JsonPropertyName("from")]
        public DateTime From { get; set; }
        [JsonPropertyName("to")]
        public DateTime To { get; set; }
        [JsonPropertyName("generationmix")]
        public List<SingleSourceData> EnergySourceData { get; set; } = new();
        public double GetCEPercentage()
        {
            if (EnergySourceData == null || !EnergySourceData.Any())
            {
                return 0;
            }
            
            var cleanSrcs = new[] { "biomass", "nuclear", "hydro", "wind", "solar" };

            return EnergySourceData.Where(es => cleanSrcs.Contains(es.Fuel.ToLower()))
                                   .Sum(es => es.Percentage);
        }
    }

    public class SingleSourceData
    {
        [JsonPropertyName("fuel")]
        public string Fuel { get; set; } = string.Empty;
        [JsonPropertyName("perc")]
        public double Percentage { get; set; }
    }
}