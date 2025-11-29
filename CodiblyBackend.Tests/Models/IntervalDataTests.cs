using CodiblyBackend.Models;
using Xunit;

namespace CodiblyBackend.Tests.Models
{
    public class IntervalDataTests
    {
        [Fact]
        public void GetCEPercentage_ShouldSumOnlyCleanEnergySources()
        {
            var interval = new IntervalData
            {
                EnergySourceData = new List<SingleSourceData>
                {
                    new() { Fuel = "biomass", Percentage = 10 },
                    new() { Fuel = "coal", Percentage = 50 },
                    new() { Fuel = "wind", Percentage = 20 },
                    new() { Fuel = "gas", Percentage = 20 } 
                }
            };

            var result = interval.GetCEPercentage();

            Assert.Equal(30, result);
        }

        [Fact]
        public void GetCEPercentage_ShouldBeCaseInsensitive()
        {
            var interval = new IntervalData
            {
                EnergySourceData = new List<SingleSourceData>
                {
                    new() { Fuel = "WiNd", Percentage = 50 },
                    new() { Fuel = "SOLAR", Percentage = 50 }
                }
            };

            var result = interval.GetCEPercentage();

            Assert.Equal(100, result);
        }

        [Fact]
        public void GetCEPercentage_ShouldReturnZero_WhenListIsEmpty()
        {
            var interval = new IntervalData { EnergySourceData = new List<SingleSourceData>() };

            var result = interval.GetCEPercentage();

            Assert.Equal(0, result);
        }

        [Fact]
        public void GetCEPercentage_ShouldReturnZero_WhenListIsNull()
        {
            var interval = new IntervalData { EnergySourceData = null };

            var result = interval.GetCEPercentage();

            Assert.Equal(0, result);
        }

        [Fact]
        public void CarbonIntensityResponse_ShouldInitializeEmptyList()
        {
            var response = new CarbonIntensityResponse();

            Assert.NotNull(response.Data);
            Assert.Empty(response.Data);
        }
    }
}