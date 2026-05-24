using Homunity_Buisness_Logic;
using Homunity_Data_Access;
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
                return BadRequest(new { message = "City is required" });

            var areas = clsLocation.GetAreasByCity(city);
            return Ok(areas);
        }

        // ================= POST — إضافة Location للعقار =================
        [HttpPost("SetPropertyLocation")]
        public IActionResult SetPropertyLocation([FromBody] SetLocationRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request" });

            if (request.PropertyId <= 0)
                return BadRequest(new { message = "Invalid PropertyId" });

            if (request.UniversityId <= 0)
                return BadRequest(new { message = "Invalid UniversityId" });

            // 1. جيب إحداثيات الجامعة
            bool uniFound = clsUniversitiesData.GetUniversityByID(
                request.UniversityId,
                out string uniName,
                out double uniLat,
                out double uniLon);

            if (!uniFound)
                return NotFound(new { message = "University not found" });

            // 2. جيب الـ Property عشان تاخد الـ LocationId
            var property = clsProperties.FindByID(request.PropertyId);
            if (property == null)
                return NotFound(new { message = "Property not found" });

            // 3. احسب المسافة في الـ Backend
            double distance = clsUniversities.CalculateDistance(
                request.Lat, request.Lng, uniLat, uniLon);

            // 4. حدّث الـ Location
            bool locationUpdated = clsLocationData.UpdateLocation(
                property.LocationID,
                request.Address,
                request.Lat,
                request.Lng);

            if (!locationUpdated)
                return StatusCode(500, new { message = "Failed to update location" });

            // 5. اربط العقار بالجامعة
            bool universityUpdated = clsLocationData.UpdatePropertyUniversity(
                request.PropertyId, request.UniversityId);

            if (!universityUpdated)
                return StatusCode(500, new { message = "Failed to link university" });

            return Ok(new
            {
                message = "Location saved successfully",
                locationId = property.LocationID,
                universityName = uniName,
                distance_km = distance
            });
        }

        // ================= PUT — تعديل Location للعقار =================
        [HttpPut("UpdatePropertyLocation")]
        public IActionResult UpdatePropertyLocation([FromBody] SetLocationRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request" });

            if (request.PropertyId <= 0)
                return BadRequest(new { message = "Invalid PropertyId" });

            if (request.UniversityId <= 0)
                return BadRequest(new { message = "Invalid UniversityId" });

            // 1. جيب إحداثيات الجامعة
            bool uniFound = clsUniversitiesData.GetUniversityByID(
                request.UniversityId,
                out string uniName,
                out double uniLat,
                out double uniLon);

            if (!uniFound)
                return NotFound(new { message = "University not found" });

            // 2. جيب الـ Property
            var property = clsProperties.FindByID(request.PropertyId);
            if (property == null)
                return NotFound(new { message = "Property not found" });

            // 3. احسب المسافة
            double distance = clsUniversities.CalculateDistance(
                request.Lat, request.Lng, uniLat, uniLon);

            // 4. حدّث الـ Location
            bool locationUpdated = clsLocationData.UpdateLocation(
                property.LocationID,
                request.Address,
                request.Lat,
                request.Lng);

            if (!locationUpdated)
                return StatusCode(500, new { message = "Failed to update location" });

            // 5. حدّث الجامعة
            bool universityUpdated = clsLocationData.UpdatePropertyUniversity(
                request.PropertyId, request.UniversityId);

            if (!universityUpdated)
                return StatusCode(500, new { message = "Failed to update university link" });

            return Ok(new
            {
                message = "Location updated successfully",
                locationId = property.LocationID,
                universityName = uniName,
                distance_km = distance
            });
        }
    }

    // ================= Request Model =================
    public class SetLocationRequest
    {
        public int PropertyId { get; set; }
        public int UniversityId { get; set; }
        public string Address { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}