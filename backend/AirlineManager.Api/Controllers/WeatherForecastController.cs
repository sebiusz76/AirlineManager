using Microsoft.AspNetCore.Mvc;

namespace AirlineManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetWeather() => Ok(new[]
        {
            new { temp = 22, condition = "Sunny" },
            new { temp = 25, condition = "Sunny" },
            new { temp = 5, condition = "Cold" },
        });
    }
}