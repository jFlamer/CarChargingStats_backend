using CodiblyBackend.Controllers;
using CodiblyBackend.Models;
using CodiblyBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq; // Biblioteka do "udawania" serwisu (Mocking)
using Xunit;

namespace CodiblyBackend.Tests
{
    public class EnergyControllerTests
    {
        private readonly Mock<InterfaceCarbonIntensityService> _mockService;
        private readonly EnergyMixController _controller;

        public EnergyControllerTests()
        {
            // Setup: Tworzymy "fałszywy" serwis i wstrzykujemy go do kontrolera przed każdym testem
            _mockService = new Mock<InterfaceCarbonIntensityService>();
            _controller = new EnergyMixController(_mockService.Object);
        }

        // --- CZĘŚĆ 1: TESTY LOGIKI BIZNESOWEJ (MODEL) ---

        [Fact]
        public void GetCEPercentage_ShouldSumOnlyCleanEnergySources()
        {
            // Arrange
            // Tworzymy interwał z mieszanką paliw
            var interval = new IntervalData
            {
                EnergySourceData = new List<SingleSourceData>
                {
                    new() { Fuel = "biomass", Percentage = 10 },  // Czysta
                    new() { Fuel = "coal", Percentage = 50 },     // Brudna
                    new() { Fuel = "wind", Percentage = 20 },     // Czysta
                    new() { Fuel = "gas", Percentage = 20 }       // Brudna
                }
            };

            // Act
            var result = interval.GetCEPercentage();

            // Assert: Oczekujemy sumy 10 (biomass) + 20 (wind) = 30
            Assert.Equal(30, result);
        }

        [Fact]
        public void GetCEPercentage_ShouldBeCaseInsensitive()
        {
            // Sprawdzamy, czy "WiNd" i "SOLAR" też zostaną policzone mimo dziwnej wielkości liter
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

        // --- CZĘŚĆ 2: TESTY WALIDACJI (KONTROLER) ---

        [Theory] // Theory pozwala uruchomić ten sam test dla wielu różnych wartości wejściowych
        [InlineData(0)]  // Za krótko (min 1h)
        [InlineData(7)]  // Za długo (max 6h)
        [InlineData(-5)] // Wartość ujemna
        public async Task CalculateBestWindow_ShouldReturnBadRequest_WhenHoursAreInvalid(int hours)
        {
            // Act
            var result = await _controller.CalculateBestWindow(new ChargingRequest { Hours = hours });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CalculateBestWindow_ShouldReturnNotFound_WhenApiReturnsEmptyData()
        {
            // Arrange: Symulujemy, że API zwróciło pustą listę (np. awaria zewnętrznego serwisu)
            _mockService.Setup(s => s.GetCIDataAsync3Days())
                        .ReturnsAsync(new List<IntervalData>());

            // Act
            var result = await _controller.CalculateBestWindow(new ChargingRequest { Hours = 2 });

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // --- CZĘŚĆ 3: TESTY ALGORYTMU (SLIDING WINDOW) ---

        [Fact]
        public async Task CalculateBestWindow_ShouldFindHighestCleanEnergyPeak()
        {
            // Arrange: Przygotowujemy dane testowe - 4 interwały (czyli 2 godziny łącznie)
            // Scenariusz:
            // 1. 10% czystej
            // 2. 90% czystej (PEAK - Start okna)
            // 3. 90% czystej (PEAK - Koniec okna)
            // 4. 10% czystej
            // Szukamy okna 1h (2 interwały). Algorytm powinien wybrać interwały nr 2 i 3.

            var baseTime = DateTime.UtcNow.Date;
            var testData = new List<IntervalData>
            {
                CreateInterval(baseTime.AddHours(1), 10),
                CreateInterval(baseTime.AddHours(1.5), 90), // <-- Oczekiwany Start
                CreateInterval(baseTime.AddHours(2), 90),   // <-- Koniec okna 1h
                CreateInterval(baseTime.AddHours(2.5), 10)
            };

            _mockService.Setup(s => s.GetCIDataAsync3Days())
                        .ReturnsAsync(testData);

            // Act
            var result = await _controller.CalculateBestWindow(new ChargingRequest { Hours = 1 });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChargingResponse>(okResult.Value);

            // Sprawdzamy czy znalazło średnią 90%
            Assert.Equal(90, response.AverageCleanEnergy);
            // Sprawdzamy czy start jest o godzinie 1.5 (czyli 1:30 od bazy)
            Assert.Equal(baseTime.AddHours(1.5), response.StartTime);
        }

        [Fact]
        public async Task CalculateBestWindow_ShouldHandleMidnightCrossing()
        {
            // Arrange: Symulujemy sytuację "przejścia przez północ".
            // To jest TRUDNY PRZYPADEK - okno zaczyna się jednego dnia, a kończy drugiego.
            // Najlepsze okno zaczyna się o 23:30 w Dniu 1 i kończy o 00:30 w Dniu 2.
            
            var day1 = DateTime.UtcNow.Date;
            var day2 = day1.AddDays(1);

            var testData = new List<IntervalData>
            {
                CreateInterval(day1.AddHours(22.0), 10),
                CreateInterval(day1.AddHours(22.5), 10),
                
                // --- OKNO OPTYMALNE (Średnia 100%) ---
                // Interwał 1: 23:30 - 00:00
                CreateInterval(day1.AddHours(23.5), 100), 
                // Interwał 2: 00:00 - 00:30 (następny dzień)
                CreateInterval(day2.AddHours(00.0), 100), 
                // -------------------------------------

                CreateInterval(day2.AddHours(00.5), 20),
                CreateInterval(day2.AddHours(01.0), 20)
            };

            _mockService.Setup(s => s.GetCIDataAsync3Days())
                        .ReturnsAsync(testData);

            // Act: Szukamy okna 1-godzinnego
            var result = await _controller.CalculateBestWindow(new ChargingRequest { Hours = 1 });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChargingResponse>(okResult.Value);

            Assert.Equal(100, response.AverageCleanEnergy);
            Assert.Equal(day1.AddHours(23.5), response.StartTime); // Powinno zwrócić 23:30 dnia pierwszego
        }

        // --- HELPERY DO TWORZENIA DANYCH ---
        
        private IntervalData CreateInterval(DateTime time, double cleanPerc)
        {
            // Pomocnicza metoda, żeby nie pisać w kółko "new List<SingleSourceData>..." w testach
            return new IntervalData
            {
                From = time,
                To = time.AddMinutes(30),
                EnergySourceData = new List<SingleSourceData>
                {
                    // Tworzymy jedno "czyste" źródło (solar) i jedno "brudne" (coal) dopełniające do 100%
                    new() { Fuel = "solar", Percentage = cleanPerc },
                    new() { Fuel = "coal", Percentage = 100 - cleanPerc }
                }
            };
        }
    }
}