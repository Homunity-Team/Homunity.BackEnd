using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Payment")]
    [ApiController]
    [Authorize]
    public class PaymentController : AuthorizedControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService) => _paymentService = paymentService;

        [HttpPost("create-order/{bookingId}")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentOrderResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder(int bookingId)
        {
            if (bookingId <= 0)
                return Problem(detail: "Invalid parameters.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var order = await _paymentService.CreateOrderAsync(bookingId, CurrentUserId);
            if (order == null)
                return Problem(detail: "Cannot create order. Booking must be Confirmed and owned by the current student.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            return Ok(order);
        }

        [HttpPost("process")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessMessageResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessPayment([FromBody] MockProcessRequest request)
        {
            if (request == null)
                return Problem(detail: "Request body is required.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var (success, message) = await _paymentService.ProcessPaymentAsync(request, CurrentUserId);
            if (!success)
            {
                if (message == "Forbidden") return Forbid();
                return Problem(detail: message, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            }

            return Ok(new SuccessMessageResponse { Message = message });
        }

        [HttpGet("status/{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentStatusResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatus(int bookingId)
        {
            if (bookingId <= 0)
                return Problem(detail: "Invalid BookingId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var isOwnerOrAdmin = User.IsInRole("Owner") || User.IsInRole("Admin");
            var status = await _paymentService.GetStatusAsync(bookingId, CurrentUserId, isOwnerOrAdmin);
            if (status == null)
                return Problem(detail: "Payment status not found or access denied.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(status);
        }
    }
}
