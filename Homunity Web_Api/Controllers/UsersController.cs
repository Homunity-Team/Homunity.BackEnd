using Homunity_Buisness_Logic;
using Homunity_Data_Access;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Homunity_Buisness_Logic.clsUsers;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Users")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        // ================= Register =================
        [HttpPost("Register", Name = "Register User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Register([FromBody] clsUsers user)
        {
            if (user == null)
                return BadRequest(new { message = "Invalid request body" });

            bool result = user.Save();

            if (!result)
            {
                if (clsUsersData.IsPhoneExists(user.Phone))
                    return Conflict(new { message = "Phone already exists" });

                return BadRequest(new { message = "Invalid user data" });
            }

            return CreatedAtAction(
                nameof(GetProfile),
                new { id = user.UserID },
                new
                {
                    user.UserID,
                    user.FirstName,
                    user.LastName,
                    user.Phone,
                    user.RoleId,
                    user.IsActive
                }
            );
        }



        // ================= Login =================
        [HttpPost("Login", Name = "Login User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Login(string phone, string password)
        {
            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { message = "Phone and password are required" });

            clsUsers user = clsUsers.Login(phone, password);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials or inactive user" });

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.RoleId
            });
        }



        // ================= Get Profile User =================
        [HttpGet("Get Profile By ID", Name = "Get Profile By ID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProfile(int id)
        {
            clsUsers user = clsUsers.GetProfile(id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Phone,
                user.RoleId,
                user.IsActive
            });
        }




        // ================= Update Status =================
        [HttpPut("Update Status By ID", Name = "Update Status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateStatus(int id, bool isActive)
        {
            bool updated = clsUsersData.UpdateUserStatus(id, isActive);

            if (!updated)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User status updated successfully" });
        }




        // ================= Delete User =================
        [HttpDelete("Delete User By ID", Name = "Delete User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            bool deleted = clsUsersData.DeleteUser(id);

            if (!deleted)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User deleted successfully" });
        }
    }
}