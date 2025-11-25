using CodiblyBackend.Models;

namespace CodiblyBackend.Services
{
    public interface InterfaceCarbonIntensityService
    {
        Task<List<IntervalData>> GetCIDataAsync3Days();
    }
}