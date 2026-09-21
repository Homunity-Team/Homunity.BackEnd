using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Booking")]
    [ApiController]
    [Authorize]
    public class BookingController : AuthorizedControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IPropertyService _propertyService;

        public BookingController(IBookingService bookingService, IPropertyService propertyService)
        {
            _bookingService = bookingService;
            _propertyService = propertyService;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BookingCreatedResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBooking(int PropertyId, int StudentId)
        {
            if (PropertyId <= 0) return Problem(detail: "Invalid PropertyId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (StudentId <= 0) return Problem(detail: "Invalid StudentId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (!await IsAuthorizedForResourceAsync(StudentId)) return Forbid();

            var result = await _bookingService.CreateAsync(PropertyId, StudentId);
            if (!result.Success)
                return Problem(detail: "Error creating booking.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");

            var response = new BookingCreatedResponse
            {
                BookingId = result.BookingId,
                PropertyId = result.PropertyId,
                StudentId = result.StudentId,
                StatusId = result.StatusId,
                CreatedAt = result.CreatedAt,
                Message = "Booking created successfully"
            };

            return CreatedAtAction(nameof(GetBookingById), new { id = result.BookingId }, response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingDetailResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingById(int id)
        {
            if (id <= 0) return Problem(detail: "Invalid BookingId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null) return Problem(detail: $"Booking {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            if (!await IsAuthorizedForResourceAsync(booking.StudentId, booking.PropertyOwnerId)) return Forbid();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = new BookingDetailResponse
            {
                BookingId = booking.BookingId,
                StatusId = booking.StatusId,
                StatusName = booking.StatusName,
                CreatedAt = booking.CreatedAt,
                ConfirmedAt = booking.ConfirmedAt,
                Property = new BookingPropertyInfo
                {
                    PropertyId = booking.PropertyId,
                    Title = booking.PropertyTitle,
                    Price = booking.PropertyPrice,
                    Address = booking.PropertyAddress,
                    ImageUrl = booking.PropertyImagePath == null ? null : $"{baseUrl}/{booking.PropertyImagePath}"
                }
            };

            return Ok(response);
        }

        [HttpGet("student/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingsByStudent(int studentId)
        {
            if (studentId <= 0) return Problem(detail: "Invalid StudentId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (!await IsAuthorizedForResourceAsync(studentId)) return Forbid();

            var bookings = await _bookingService.GetByStudentIdAsync(studentId);
            if (bookings.Count == 0)
                return Problem(detail: $"No bookings for student {studentId}.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = bookings.Select(b => new BookingListItemResponse
            {
                BookingId = b.BookingId,
                StatusId = b.StatusId,
                StatusName = b.StatusName,
                CreatedAt = b.CreatedAt,
                ConfirmedAt = b.ConfirmedAt,
                Property = new BookingListPropertyInfo
                {
                    PropertyId = b.PropertyId,
                    Title = b.PropertyTitle,
                    Price = b.PropertyPrice,
                    Address = b.PropertyAddress,
                    ImageUrl = b.ImagePath == null ? null : $"{baseUrl}/{b.ImagePath}"
                }
            }).ToList();

            return Ok(result);
        }

        [HttpGet("owner/{ownerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingsByOwner(int ownerId)
        {
            if (ownerId <= 0) return Problem(detail: "Invalid OwnerId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (!await IsAuthorizedForResourceAsync(ownerId)) return Forbid();

            var bookings = await _bookingService.GetByOwnerIdAsync(ownerId);
            if (bookings.Count == 0)
                return Problem(detail: $"No bookings for owner {ownerId}.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = bookings.Select(b => new BookingListItemResponse
            {
                BookingId = b.BookingId,
                StudentName = b.StudentName,
                StatusId = b.StatusId,
                StatusName = b.StatusName,
                CreatedAt = b.CreatedAt,
                ConfirmedAt = b.ConfirmedAt,
                Property = new BookingListPropertyInfo
                {
                    PropertyId = b.PropertyId,
                    Title = b.PropertyTitle,
                    Price = null, // نفس السلوك الأصلي: السعر غير موجود إطلاقًا في سياق قائمة حجوزات المالك
                    Address = b.PropertyAddress,
                    ImageUrl = b.ImagePath == null ? null : $"{baseUrl}/{b.ImagePath}"
                }
            }).ToList();

            return Ok(new { message = "Bookings retrieved successfully", count = result.Count, bookings = result });
        }

        [HttpGet("property/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookingsByProperty(int propertyId)
        {
            if (propertyId <= 0) return Problem(detail: "Invalid PropertyId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var ownership = await _propertyService.GetOwnershipAsync(propertyId);
            if (ownership == null) return Problem(detail: "Property not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");
            if (!await IsAuthorizedForResourceAsync(ownership.OwnerId)) return Forbid();

            var bookings = await _bookingService.GetByPropertyIdAsync(propertyId);
            if (bookings.Count == 0) return Ok(new { message = "No bookings found", bookings = new List<BookingByPropertyItemResponse>() });

            var result = bookings.Select(b => new BookingByPropertyItemResponse
            {
                BookingId = b.BookingId,
                StudentName = b.StudentName,
                CheckInDate = b.CreatedAt.ToString("yyyy-MM-dd"),
                StatusName = b.StatusName
            }).ToList();

            return Ok(new { message = "Bookings retrieved successfully", count = result.Count, bookings = result });
        }

        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Owner,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingConfirmedResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmBooking(int id)
        {
            if (id <= 0) return Problem(detail: "Invalid BookingId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            var OwnerId = CurrentUserId;
            if (OwnerId <= 0) return Problem(detail: "Invalid OwnerId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (!await IsAuthorizedForResourceAsync(OwnerId)) return Forbid();

            var result = await _bookingService.ConfirmAsync(id, OwnerId);

            if (result.NotFoundFlag) return Problem(detail: $"Booking {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");
            if (!result.Success) return Problem(detail: "Error confirming booking.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");

            return Ok(new BookingConfirmedResponse
            {
                BookingId = result.BookingId,
                StatusId = result.StatusId,
                ConfirmedAt = result.ConfirmedAt,
                Message = "Booking confirmed successfully."
            });
        }

        [HttpPut("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingCancelledResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelBooking(int id)
        {
            if (id <= 0) return Problem(detail: "Invalid BookingId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null) return Problem(detail: $"Booking {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");
            if (!await IsAuthorizedForResourceAsync(booking.StudentId)) return Forbid();

            var result = await _bookingService.CancelAsync(id);
            if (!result.Success) return Problem(detail: "Cannot cancel a confirmed or already cancelled booking.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");

            return Ok(new BookingCancelledResponse
            {
                BookingId = result.BookingId,
                StatusId = result.StatusId,
                Message = "Booking cancelled successfully"
            });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id) =>
            Problem(detail: "Use PUT /api/Booking/{id}/cancel instead.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
    }
}