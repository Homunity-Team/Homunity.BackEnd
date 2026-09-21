using Homunity_Data_Access.Repositories;
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
        private readonly IBookingStatusRepository _repo;
        public BookingStatusController(IBookingStatusRepository repo) => _repo = repo;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllBookingStatuses()
        {
            var statuses = await _repo.GetAllAsync();
            if (statuses.Count == 0) return Problem(detail: "No booking statuses found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(statuses.Select(s => new BookingStatusResponse { BookingStatusId = s.BookingStatusId, StatusName = s.StatusName }).ToList());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingStatusResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingStatusById(int id)
        {
            if (id <= 0) return Problem(detail: "Invalid BookingStatusId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var status = await _repo.FindAsync(id);
            if (status == null) return Problem(detail: $"Booking status with ID {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(new BookingStatusResponse { BookingStatusId = status.BookingStatusId, StatusName = status.StatusName });
        }
    }
}