using CodiblyBackend.Controllers;
using CodiblyBackend.Models;
using CodiblyBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CodiblyBackend.Tests.Controllers
{
    public class EnergyControllerTests
    {
        private readonly Mock<InterfaceCarbonIntensityService> _mockService;
        private readonly EnergyMixController _controller;

        public EnergyControllerTests()
        {
            _mockService = new Mock<InterfaceCarbonIntensityService>();
            _controller = new EnergyMixController(_mockService.Object);
        }

        [Theory]
        [InlineData(0)]  
        [InlineData(7)] 
        [InlineData(-1)] 
        public async Task CalculateBestWindow_ShouldReturnBadRequest_WhenHoursInvalid(int hours)
        {

            var result = await _controller.CalculateBestWindow(new ChargingRequest { Hours = hours });


            Assert.IsType<BadRequestObjectResult>(result);
        }


        [Fact]
        public async Task CalculateBestWindow_ShouldReturnNotFound_WhenServiceReturnsNull()
        {
            _mockService.Setup(s => s.GetCIDataAsync3Days())
                        .ReturnsAsync((List<IntervalData>)null);

            var result = await _controller.CalculateBestWindow(new ChargingRequest { Hours = 1 });

            Assert.IsType<NotFoundObjectResult>(result);
        }


        [Fact]
        public async Task CalculateBestWindow_ShouldIgnorePastData()
        {
            

            var now = DateTime.UtcNow; 
            var testData = new List<IntervalData>
            {
                CreateInterval(now.AddHours(-2), 100), 
                CreateInterval(now.AddHours(-1.5), 100),

                CreateInterval(now.AddHours(1), 50),
                CreateInterval(now.AddHours(1.5), 50)
            };

            _mockService.Setup(s => s.GetCIDataAsync3Days()).ReturnsAsync(testData);

            var result = await _controller.CalculateBestWindow(new ChargingRequest { Hours = 1 });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChargingResponse>(okResult.Value);


            Assert.True(response.StartTime >= now, "Błąd: Wybrano datę z przeszłości!");
            Assert.Equal(50, response.AverageCleanEnergy);
        }

        [Fact]
        public async Task CalculateBestWindow_ShouldReturnNotFound_WhenNotEnoughFutureData()
        {
            
            var now = DateTime.UtcNow;
            var testData = new List<IntervalData>
            {
                CreateInterval(now.AddHours(0.5), 80),
                CreateInterval(now.AddHours(1.0), 80)
            };

            _mockService.Setup(s => s.GetCIDataAsync3Days()).ReturnsAsync(testData);

            var result = await _controller.CalculateBestWindow(new ChargingRequest { Hours = 3 });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CalculateBestWindow_ShouldFindWindowCrossingMidnight()
        {
            var now = DateTime.UtcNow;
            
            var futureStart = now.AddHours(2); 
            var testData = new List<IntervalData>
            {
                CreateInterval(now.AddHours(0.5), 10), 
                CreateInterval(now.AddHours(1.0), 10), 
                

                CreateInterval(futureStart, 100), 
                CreateInterval(futureStart.AddMinutes(30), 100), 

                CreateInterval(futureStart.AddMinutes(60), 20),
            };

            _mockService.Setup(s => s.GetCIDataAsync3Days()).ReturnsAsync(testData);

            var result = await _controller.CalculateBestWindow(new ChargingRequest { Hours = 1 });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChargingResponse>(okResult.Value);

            Assert.Equal(100, response.AverageCleanEnergy);
            Assert.Equal(futureStart, response.StartTime);
        }


        [Fact]
        public async Task GetDailyMix_ShouldReturnGroupedData_ForThreeDays()
        {

            var now = DateTime.UtcNow;
            
            var testData = new List<IntervalData>
            {
                CreateInterval(now.AddHours(1), 50),
                CreateInterval(now.AddHours(2), 50),
                
                CreateInterval(now.AddDays(1).AddHours(1), 80),

                CreateInterval(now.AddDays(2).AddHours(1), 20)
            };

            _mockService.Setup(s => s.GetCIDataAsync3Days())
                        .ReturnsAsync(testData);

            var result = await _controller.GetDailyMix();

            var okResult = Assert.IsType<OkObjectResult>(result);
            
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            
            Assert.Equal(3, list.Count());
        }

        [Fact]
        public async Task GetDailyMix_ShouldReturnNotFound_WhenNoData()
        {
            _mockService.Setup(s => s.GetCIDataAsync3Days())
                        .ReturnsAsync(new List<IntervalData>()); 
            var result = await _controller.GetDailyMix();

            Assert.IsType<NotFoundObjectResult>(result);
        }
        

        private IntervalData CreateInterval(DateTime time, double cleanPerc)
        {
            return new IntervalData
            {
                From = time,
                To = time.AddMinutes(30),
                EnergySourceData = new List<SingleSourceData>
                {
                    new() { Fuel = "solar", Percentage = cleanPerc },
                    new() { Fuel = "coal", Percentage = 100 - cleanPerc }
                }
            };
        }
    }
}