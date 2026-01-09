using Homunity_Buisness_Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Roles")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        [HttpGet("Get All Roles", Name = "Get All Roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAllRoles()
        {
           
              DataTable dt = clsRoles.GetRoles();

              if (dt == null)
                  return StatusCode(500, new { message = "Failed to load roles" });

              List<object> roles = new List<object>();

              foreach (DataRow row in dt.Rows)
              {
                  roles.Add(new
                  {
                      RoleId = row["RoleId"],
                      Name = row["Name"]
                  });
              }

              return Ok(roles);
           

        }

    }   
}
