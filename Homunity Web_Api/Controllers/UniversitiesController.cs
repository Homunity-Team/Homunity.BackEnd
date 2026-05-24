using Homunity_Buisness_Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Universities")]
    [ApiController]
    public class UniversitiesController : ControllerBase
    {
        [HttpGet("GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var universities = clsUniversities.GetAllUniversities();

            return Ok(new
            {
                message = "Universities retrieved successfully",
                count = universities.Count,
                universities = universities.Select(u => new
                {
                    universityId = u.UniversityId,
                    name = u.Name,
                    latitude = u.Latitude,
                    longitude = u.Longitude
                })
            });
        }
    }
}