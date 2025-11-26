using CodiblyBackend.Models;
using CodiblyBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodiblyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnergyMixController : ControllerBase
    {
        private readonly InterfaceCarbonIntensityService _service;

        public EnergyMixController(InterfaceCarbonIntensityService service)
        {
            _service = service;
        }

        // ENDPOINT 1: Wyświetlanie miksu energetycznego (średnie dla dni)
        // GET: api/EnergyMix/daily-mix
        [HttpGet("daily-mix")]
        public async Task<IActionResult> GetDailyMix()
        {
            var data = await _service.GetCIDataAsync3Days();
            
            if (data == null || !data.Any())
            {
                return NotFound("No data available.");
            }

            var summaryGroup = data.GroupBy(i => i.From.Date)
            .Select(summaryGroup => new
            {
                Date = summaryGroup.Key,
                AvgValue = Math.Round(summaryGroup.Average(x => x.GetCEPercentage()), 2),
                FuelMix = summaryGroup
                    .SelectMany(x => x.EnergySourceData)
                    .GroupBy(f => f.Fuel)
                    .Select(global => new
                    {
                        Fuel = global.Key,
                        Percentage = Math.Round(global.Average(f => f.Percentage), 1)
                    }).ToList()
            }).ToList();

            var dailySummaries = summaryGroup
            .Select(group => new
            {
                Date = group.Date,
                AverageCEPercentage = group.AvgValue,
                FuelMix = group.FuelMix,
                msg = $"Average clean energy percentage for {group.Date:yyyy-MM-dd} is {group.AvgValue}%"
            }).OrderBy(x => x.Date)
            .Take(3).ToList();

            return Ok(dailySummaries);
        }

        // ENDPOINT 2: Obliczanie optymalnego okna ładowania
        // POST: api/EnergyMix/optimal-charging
        [HttpPost("optimal-charging")]
        public async Task<IActionResult> CalculateBestWindow([FromBody] ChargingRequest request)
        {
            if (request.Hours < 1 || request.Hours > 6)
            {
                return BadRequest("Charging hours must be between 1 and 6.");
            }

            var data = await _service.GetCIDataAsync3Days();
            if (data == null || !data.Any())
            {
                return NotFound("No data available.");
            }

            // Sliding Window Algorithm
            int intervalsNeeded = request.Hours * 2; // 30-min intervals
            var dataSorted = data.OrderBy(d => d.From).ToList();

            double maxCEPercentageAvg = -1;
            IntervalData? bestStartInterval = null;

            for (int i = 0; i <= dataSorted.Count - intervalsNeeded; i++)
            {
                var window = dataSorted.GetRange(i, intervalsNeeded);

                double currentAvg = window.Average(i => i.GetCEPercentage());

                if (currentAvg > maxCEPercentageAvg)
                {
                    maxCEPercentageAvg = currentAvg;
                    bestStartInterval = window.First();
                }
            }

            if (bestStartInterval == null)
            {
                return NotFound("No suitable charging window found.");
            }

            return Ok(new ChargingResponse
            {
                StartTime = bestStartInterval.From,
                EndTime = bestStartInterval.From.AddHours(request.Hours),
                AverageCleanEnergy = Math.Round(maxCEPercentageAvg, 2)
            });

        }
    }
}