using Homunity_Buisness_Logic;
using Homunity_Data_Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Location")]
    [ApiController]
    [Authorize]
    public class LocationController : AuthorizedControllerBase
    {
        [HttpGet("cities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCities()
        {
            var cities = clsLocation.GetCities();
            return Ok(cities);
        }

        [HttpGet("areas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetAreas([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return Problem(detail: "City is required.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var areas = clsLocation.GetAreasByCity(city);
            return Ok(areas);
        }

        [HttpPost("SetPropertyLocation")]
        [Authorize(Roles = "Owner,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult SetPropertyLocation([FromBody] SetLocationRequest request)
        {
            if (request == null)
                return Problem(detail: "Invalid request.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (request.PropertyId <= 0)
                return Problem(detail: "Invalid PropertyId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (request.UniversityId <= 0)
                return Problem(detail: "Invalid UniversityId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            bool uniFound = clsUniversitiesData.GetUniversityByID(request.UniversityId, out string uniName, out double uniLat, out double uniLon);
            if (!uniFound)
                return Problem(detail: "University not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var property = clsProperties.FindByID(request.PropertyId);
            if (property == null)
                return Problem(detail: "Property not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");
            if (property.OwnerID != CurrentUserId && !IsAdmin) return Forbid();

            double distance = clsUniversities.CalculateDistance(request.Lat, request.Lng, uniLat, uniLon);

            bool locationUpdated = clsLocationData.UpdateLocation(property.LocationID, request.Address, request.Lat, request.Lng);
            if (!locationUpdated)
                return Problem(detail: "Failed to update location.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");

            bool universityUpdated = clsLocationData.UpdatePropertyUniversity(request.PropertyId, request.UniversityId);
            if (!universityUpdated)
                return Problem(detail: "Failed to link university.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");

            return Ok(new { message = "Location saved successfully", locationId = property.LocationID, universityName = uniName, distance_km = distance });
        }

        [HttpPut("UpdatePropertyLocation")]
        [Authorize(Roles = "Owner,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePropertyLocation([FromBody] SetLocationRequest request)
        {
            if (request == null)
                return Problem(detail: "Invalid request.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (request.PropertyId <= 0)
                return Problem(detail: "Invalid PropertyId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (request.UniversityId <= 0)
                return Problem(detail: "Invalid UniversityId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            bool uniFound = clsUniversitiesData.GetUniversityByID(request.UniversityId, out string uniName, out double uniLat, out double uniLon);
            if (!uniFound)
                return Problem(detail: "University not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var property = clsProperties.FindByID(request.PropertyId);
            if (property == null)
                return Problem(detail: "Property not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");
            if (property.OwnerID != CurrentUserId && !IsAdmin) return Forbid();

            double distance = clsUniversities.CalculateDistance(request.Lat, request.Lng, uniLat, uniLon);

            bool locationUpdated = clsLocationData.UpdateLocation(property.LocationID, request.Address, request.Lat, request.Lng);
            if (!locationUpdated)
                return Problem(detail: "Failed to update location.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");

            bool universityUpdated = clsLocationData.UpdatePropertyUniversity(request.PropertyId, request.UniversityId);
            if (!universityUpdated)
                return Problem(detail: "Failed to update university link.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");

            return Ok(new { message = "Location updated successfully", locationId = property.LocationID, universityName = uniName, distance_km = distance });
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