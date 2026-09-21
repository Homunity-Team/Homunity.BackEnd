using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/BookingStatus")]
    [ApiController]
    [Authorize]
    public class BookingStatusController : AuthorizedControllerBase
    {
        private readonly IReferenceDataService _referenceData;
        public BookingStatusController(IReferenceDataService referenceData) => _referenceData = referenceData;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllBookingStatuses()
        {
            var statuses = await _referenceData.GetBookingStatusesAsync();
            if (statuses.Count == 0) return Problem(detail: "No booking statuses found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(statuses.Select(s => new BookingStatusResponse { BookingStatusId = s.BookingStatusId, StatusName = s.StatusName }).ToList());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingStatusResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingStatusById(int id)
        {
            if (id <= 0) return Problem(detail: "Invalid BookingStatusId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var status = await _referenceData.GetBookingStatusByIdAsync(id);
            if (status == null) return Problem(detail: $"Booking status with ID {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(new BookingStatusResponse { BookingStatusId = status.BookingStatusId, StatusName = status.StatusName });
        }
    }
}
