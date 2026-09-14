using Homunity_Buisness_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Roles")]
    [ApiController]
    [Authorize]
    public class RolesController : AuthorizedControllerBase
    {
        [HttpGet("Get All Roles", Name = "Get All Roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAllRoles()
        {
            DataTable dt = clsRoles.GetRoles();
            if (dt == null)
                return Problem(detail: "Failed to load roles.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");

            var roles = new List<object>();
            foreach (DataRow row in dt.Rows)
                roles.Add(new { RoleId = row["RoleId"], Name = row["Name"] });

            return Ok(roles);
        }
    }
}