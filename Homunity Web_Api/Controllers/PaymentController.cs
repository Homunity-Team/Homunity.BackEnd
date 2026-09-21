using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Payment")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class PaymentController : AuthorizedControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService) => _paymentService = paymentService;

        [HttpPost("create-order/{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentOrderResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder(int bookingId, [FromQuery] int studentId)
        {
            if (bookingId <= 0 || studentId <= 0)
                return Problem(detail: "Invalid parameters.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (!await IsAuthorizedForResourceAsync(studentId)) return Forbid();

            var order = await _paymentService.CreateOrderAsync(bookingId, studentId);
            if (order == null)
                return Problem(detail: "Cannot create order. Booking must be in Confirmed status.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            return Ok(order);
        }

        // أضف: using Homunity_Shared_DTOs;

        [HttpPost("process")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessMessageResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessPayment([FromBody] MockProcessRequest request)
        {
            if (request == null)
                return Problem(detail: "Request body is required.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var (success, message) = await _paymentService.ProcessPaymentAsync(request);
            if (!success)
                return Problem(detail: message, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            return Ok(new SuccessMessageResponse { Success = true, Message = message });
        }
        [HttpGet("status/{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentStatusResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatus(int bookingId)
        {
            if (bookingId <= 0)
                return Problem(detail: "Invalid bookingId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var status = await _paymentService.GetStatusAsync(bookingId);
            if (status == null)
                return Problem(detail: "Booking not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(status);
        }
    }
}