using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly clsChat _chat;

        public ChatController(clsChat chat)
        {
            _chat = chat;
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request.StudentId <= 0 || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "StudentId and Message are required." });

            if (request.Message.Length > 500)
                return BadRequest(new { message = "Message too long. Max 500 characters." });

            var response = await _chat.SendMessage(request.StudentId, request.Message);
            return Ok(response);
        }

        [HttpGet("history/{studentId}")]
        public IActionResult GetHistory(int studentId)
        {
            if (studentId <= 0) return BadRequest(new { message = "Invalid StudentId." });
            var history = _chat.GetHistory(studentId);
            return Ok(new { studentId, messages = history });
        }

        [HttpDelete("clear/{studentId}")]
        public IActionResult ClearHistory(int studentId)
        {
            if (studentId <= 0) return BadRequest(new { message = "Invalid StudentId." });
            bool result = _chat.ClearHistory(studentId);
            return Ok(new { success = result, message = result ? "History cleared." : "Error clearing history." });
        }
    }
}