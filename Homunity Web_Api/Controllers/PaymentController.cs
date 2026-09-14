using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Payment")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class PaymentController : AuthorizedControllerBase
    {
        [HttpPost("create-order/{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentOrderResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateOrder(int bookingId, [FromQuery] int studentId)
        {
            if (bookingId <= 0 || studentId <= 0)
                return Problem(detail: "Invalid parameters.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (studentId != CurrentUserId) return Forbid();

            var order = clsPayment.CreateOrder(bookingId, studentId);
            if (order == null)
                return Problem(detail: "Cannot create order. Booking must be in Confirmed status.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            return Ok(order);
        }

        [HttpPost("process")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ProcessPayment([FromBody] MockProcessRequest request)
        {
            if (request == null)
                return Problem(detail: "Request body is required.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var (success, message) = clsPayment.ProcessPayment(request);
            if (!success)
                return Problem(detail: message, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            return Ok(new { success = true, message });
        }

        [HttpGet("status/{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentStatusResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetStatus(int bookingId)
        {
            if (bookingId <= 0)
                return Problem(detail: "Invalid bookingId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var status = clsPayment.GetStatus(bookingId);
            if (status == null)
                return Problem(detail: "Booking not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(status);
        }
    }
}