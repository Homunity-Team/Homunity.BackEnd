using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs;
using Homunity_Shared_DTOs.Users;
using Homunity_Web_Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Users")]
    [ApiController]
    [Authorize]
    public class UsersController : AuthorizedControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUsersService usersService, ILogger<UsersController> logger)
        {
            _usersService = usersService;
            _logger = logger;
        }

        [HttpPost("Register", Name = "Register User")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var (success, phoneConflict, user) = await _usersService.RegisterAsync(request);

            if (!success)
            {
                if (phoneConflict)
                    _logger.LogWarning("User registration rejected: phone already exists");

                return phoneConflict
                    ? Problem(detail: "Phone already exists.", statusCode: StatusCodes.Status409Conflict, title: "Conflict")
                    : Problem(detail: "Invalid user data.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            }

            _logger.LogInformation("User registered successfully: UserId {UserId}, RoleId {RoleId}", user.UserID, user.RoleId);

            return CreatedAtAction(nameof(GetProfile), new { id = user.UserID }, user);
        }

        [HttpPut("Update Status By ID", Name = "Update Status")]
        [Authorize(Policy = PolicyNames.AdminOnly)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessMessageResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            bool updated = await _usersService.UpdateStatusAsync(id, isActive);
            if (!updated)
                return Problem(detail: $"User {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");
            return Ok(new SuccessMessageResponse { Message = "User status updated successfully" });
        }

        [HttpDelete("Delete User By ID", Name = "Delete User")]
        [Authorize(Policy = PolicyNames.AdminOnly)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessMessageResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _usersService.DeleteAsync(id);
            if (!deleted)
                return Problem(detail: $"User {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");
            return Ok(new SuccessMessageResponse { Message = "User deleted successfully" });
        }

        [HttpGet("Get Profile By ID", Name = "Get Profile By ID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile(int id)
        {
            if (!await IsAuthorizedForResourceAsync(id)) return Forbid();

            var user = await _usersService.GetProfileAsync(id);

            if (user == null)
                return Problem(detail: $"User {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(user);
        }
    }
}