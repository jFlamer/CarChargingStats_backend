using CodiblyBackend.Controllers;
using CodiblyBackend.Models;
using CodiblyBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq; // Biblioteka do "udawania" serwisu (Mocking)
using Xunit;

namespace CodiblyBackend.Tests
{
    public class EnergyCalculatorTests
    {
        // TEST 1: Sprawdza czy dobrze sumujemy czystą energię z pojedynczego kawałka JSONa
        [Fact]
        public void GetCEPercentage_ShouldCalculateCorrectly()
        {
            // Arrange (Przygotowanie danych - takie jak wysłałaś w czacie)
            var interval = new IntervalData
            {
                EnergySourceData = new List<SingleSourceData>
                {
                    new() { Fuel = "biomass", Percentage = 9.7 },
                    new() { Fuel = "coal", Percentage = 0 },
                    new() { Fuel = "gas", Percentage = 36.5 },
                    new() { Fuel = "nuclear", Percentage = 12.0 },
                    new() { Fuel = "wind", Percentage = 39.6 }
                }
            };

            // Act (Wykonanie)
            var result = interval.GetCEPercentage();

            // Assert (Sprawdzenie)
            // Oczekujemy 61.3 (z tolerancją do 1 miejsca po przecinku)
            Assert.Equal(61.3, result, 1);
        }
    }
}