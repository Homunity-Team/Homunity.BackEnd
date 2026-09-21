using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs.Reference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Roles")]
    [ApiController]
    [Authorize]
    public class RolesController : AuthorizedControllerBase
    {
        private readonly IReferenceDataService _referenceData;
        public RolesController(IReferenceDataService referenceData) => _referenceData = referenceData;

        [HttpGet("Get All Roles", Name = "Get All Roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _referenceData.GetRolesAsync();
            if (roles == null)
                return Problem(detail: "Failed to load roles.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");

            return Ok(roles.Select(r => new RoleResponse { RoleId = r.RoleId, Name = r.Name }).ToList());
        }
    }
}
