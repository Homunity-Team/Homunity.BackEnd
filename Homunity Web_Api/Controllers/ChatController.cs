using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class ChatController : AuthorizedControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService) => _chatService = chatService;

        [HttpPost("message")]
        [EnableRateLimiting("ChatPolicy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request.StudentId <= 0 || string.IsNullOrWhiteSpace(request.Message))
                return Problem(detail: "StudentId and Message are required.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (request.Message.Length > 500)
                return Problem(detail: "Message too long. Max 500 characters.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (!await IsAuthorizedForResourceAsync(request.StudentId)) return Forbid();

            var response = await _chatService.SendMessageAsync(request.StudentId, request.Message);
            return Ok(response);
        }

        [HttpGet("history/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistory(int studentId)
        {
            if (studentId <= 0)
                return Problem(detail: "Invalid StudentId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (!await IsAuthorizedForResourceAsync(studentId)) return Forbid();

            var history = await _chatService.GetHistoryAsync(studentId);
            return Ok(new { studentId, messages = history });
        }



 
        [HttpDelete("clear/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessMessageResponse))]
        public async Task<IActionResult> ClearHistory(int studentId)
        {
            if (studentId <= 0)
                return Problem(detail: "Invalid StudentId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (!await IsAuthorizedForResourceAsync(studentId)) return Forbid();

            bool result = await _chatService.ClearHistoryAsync(studentId);
            return Ok(new SuccessMessageResponse { Success = result, Message = result ? "History cleared." : "Error clearing history." });
        }
    }
}