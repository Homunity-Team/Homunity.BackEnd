using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs.Auth;
using Homunity_Shared_DTOs.Users;
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
        /// Authenticates a user using phone and password from the request body and returns a JWT access token.
        /// </summary>
        [HttpPost("Login", Name = "Auth Login")]
        [EnableRateLimiting("AuthPolicy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Phone) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Problem(
                    detail: "Phone and password are required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");
            }

            var (success, user, token, expiresAt) =
                await _authService.LoginAsync(request.Phone, request.Password);

            if (!success)
            {
                _logger.LogWarning(
                    "Login endpoint rejected request for phone ending in {PhoneSuffix}",
                    request.Phone.Length >= 4 ? request.Phone[^4..] : "****");

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
