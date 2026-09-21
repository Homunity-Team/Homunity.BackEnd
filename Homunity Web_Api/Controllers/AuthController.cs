using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs.Auth;
using Homunity_Shared_DTOs.Users;
using Homunity_Web_Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user using their phone number and password and returns a JWT access token when the credentials are valid.
        /// </summary>
        /// <param name="phone">The user's registered phone number.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>User information together with the JWT token and its expiration time.</returns>
        /// <response code="200">The credentials are valid and a JWT token is returned.</response>
        /// <response code="400">The phone number or password is missing.</response>
        /// <response code="401">The credentials are invalid or the user is inactive.</response>
        /// <response code="429">The request was rejected by the authentication rate limit.</response>
        [HttpPost("Login", Name = "Auth Login")]
        [EnableRateLimiting("AuthPolicy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login(string phone, string password)
        {
            if (string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password))
            {
                return Problem(
                    detail: "Phone and password are required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");
            }

            var (success, user, token, expiresAt) =
                await _authService.LoginAsync(phone, password);

            if (!success)
            {
                _logger.LogWarning(
                    "Login endpoint rejected request for phone ending in {PhoneSuffix}",
                    phone.Length >= 4 ? phone[^4..] : "****");

                return Problem(
                    detail: "Invalid credentials or inactive user.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized");
            }


            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserData = user
            });

        
        }
    }
}
