using Homunity_Buisness_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Location")]
    [ApiController]
    [Authorize]
    public class LocationController : AuthorizedControllerBase
    {
        private readonly ILocationService _locationService;
        public LocationController(ILocationService locationService) => _locationService = locationService;

        [HttpGet("cities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _locationService.GetCitiesAsync();
            return Ok(cities);
        }

        [HttpGet("areas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAreas([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return Problem(detail: "City is required.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var areas = await _locationService.GetAreasByCityAsync(city);   // بترجّع List<AreaResponse> الآن
            return Ok(areas);
        }
        [HttpPost("SetPropertyLocation")]
        [Authorize(Roles = "Owner,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPropertyLocation([FromBody] SetLocationRequest request)
        {
            if (request == null) return Problem(detail: "Invalid request.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (request.PropertyId <= 0) return Problem(detail: "Invalid PropertyId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (request.UniversityId <= 0) return Problem(detail: "Invalid UniversityId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var result = await _locationService.SetPropertyLocationAsync(request.PropertyId, request.UniversityId, request.Address, request.Lat, request.Lng, CurrentUserId, IsAdmin);
            return _BuildLocationResponse(result, "Location saved successfully");
        }

        [HttpPut("UpdatePropertyLocation")]
        [Authorize(Roles = "Owner,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePropertyLocation([FromBody] SetLocationRequest request)
        {
            if (request == null) return Problem(detail: "Invalid request.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (request.PropertyId <= 0) return Problem(detail: "Invalid PropertyId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (request.UniversityId <= 0) return Problem(detail: "Invalid UniversityId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var result = await _locationService.UpdatePropertyLocationAsync(request.PropertyId, request.UniversityId, request.Address, request.Lat, request.Lng, CurrentUserId, IsAdmin);
            return _BuildLocationResponse(result, "Location updated successfully");
        }

        private IActionResult _BuildLocationResponse(LocationUpdateResult result, string successMessage)
        {
            return result.Status switch
            {
                LocationUpdateStatus.UniversityNotFound => Problem(detail: "University not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found"),
                LocationUpdateStatus.PropertyNotFound => Problem(detail: "Property not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found"),
                LocationUpdateStatus.Forbidden => Forbid(),
                LocationUpdateStatus.Failed => Problem(detail: "Failed to update location.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error"),
                LocationUpdateStatus.Success => Ok(new { message = successMessage, locationId = result.LocationId, universityName = result.UniversityName, distance_km = result.DistanceKm }),
                _ => Problem(detail: "Unexpected error.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error")
            };
        }
    }

    public class SetLocationRequest
    {
        public int PropertyId { get; set; }
        public int UniversityId { get; set; }
        public string Address { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}