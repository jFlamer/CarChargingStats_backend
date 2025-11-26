namespace CodiblyBackend.Models
{
    public class ChargingRequest
    {
        public int Hours { get; set; }
    
    }

    public class ChargingResponse
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double AverageCleanEnergy { get; set; }
    }
}