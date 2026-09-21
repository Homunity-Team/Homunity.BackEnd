using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs.Reference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Universities")]
    [ApiController]
    [Authorize]
    public class UniversitiesController : AuthorizedControllerBase
    {
        private readonly IUniversityService _universityService;
        public UniversitiesController(IUniversityService universityService) => _universityService = universityService;

        [HttpGet("GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var universities = await _universityService.GetAllAsync();
            var result = universities.Select(u => new UniversityResponse { UniversityId = u.UniversityId, Name = u.Name, Latitude = u.Latitude, Longitude = u.Longitude }).ToList();

            return Ok(new { message = "Universities retrieved successfully", count = result.Count, universities = result });
        }
    }
}