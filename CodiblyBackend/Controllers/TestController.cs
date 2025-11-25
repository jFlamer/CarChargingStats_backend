using CodiblyBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodiblyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly InterfaceCarbonIntensityService _service;

        // Wstrzykujemy Twój serwis
        public TestController(InterfaceCarbonIntensityService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> CheckIfItWorks()
        {
            // Wywołujemy metodę, którą napisałaś
            var data = await _service.GetCIDataAsync3Days();

            // Zwracamy wynik bezpośrednio do przeglądarki
            return Ok(data);
        }
    }
}