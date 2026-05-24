using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {


        /*
        // Owner confirms booking → status: Confirmed
        // PUT /api/Payment/confirm/{bookingId}?ownerId=X
        [HttpPut("confirm/{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ConfirmBooking(int bookingId, [FromQuery] int ownerId)
        {
            if (bookingId <= 0 || ownerId <= 0)
                return BadRequest(new { message = "Invalid parameters." });

        bool result = clsPayment.ConfirmBooking(bookingId, ownerId);

            if (!result)
                return BadRequest(new { message = "Could not confirm. Check bookingId and ownerId." });

            return Ok(new { message = "Booking confirmed. Student can now proceed to payment.", bookingId });
        }

*/

        // Student creates payment order → returns mock order details
        // POST /api/Payment/create-order/{bookingId}?studentId=X
        [HttpPost("create-order/{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentOrderResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CreateOrder(int bookingId, [FromQuery] int studentId)
        {
            if (bookingId <= 0 || studentId <= 0)
                return BadRequest(new { message = "Invalid parameters." });

            var order = clsPayment.CreateOrder(bookingId, studentId);

            if (order == null)
                return BadRequest(new { message = "Cannot create order. Booking must be in Confirmed status." });

            return Ok(order);
        }

        // Student processes mock payment
        // POST /api/Payment/process
        [HttpPost("process")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ProcessPayment([FromBody] MockProcessRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var (success, message) = clsPayment.ProcessPayment(request);

            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        // Get payment status for a booking
        // GET /api/Payment/status/{bookingId}
        [HttpGet("status/{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentStatusResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetStatus(int bookingId)
        {
            if (bookingId <= 0)
                return BadRequest(new { message = "Invalid bookingId." });

            var status = clsPayment.GetStatus(bookingId);

            if (status == null)
                return NotFound(new { message = "Booking not found." });

            return Ok(status);
        }
    }
}
