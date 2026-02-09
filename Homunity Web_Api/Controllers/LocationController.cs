using Homunity_Buisness_Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        [HttpGet("cities")]
        public IActionResult GetCities()
        {
            var cities = clsLocation.GetCities();
            return Ok(cities);
        }

        [HttpGet("areas")]
        public IActionResult GetAreas([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("City is required");

            var areas = clsLocation.GetAreasByCity(city);
            return Ok(areas);
        }
    }
}
