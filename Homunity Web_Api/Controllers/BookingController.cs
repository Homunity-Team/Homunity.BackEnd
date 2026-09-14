using Homunity_Buisness_Logic;
using Homunity_Business_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Booking")]
    [ApiController]
    [Authorize]
    public class BookingController : AuthorizedControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Student")]
        public IActionResult CreateBooking(int PropertyId, int StudentId)
        {
            if (PropertyId <= 0)
                return Problem(detail: "Invalid PropertyId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (StudentId <= 0)
                return Problem(detail: "Invalid StudentId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (StudentId != CurrentUserId) return Forbid();

            clsBooking booking = new clsBooking { PropertyId = PropertyId, StudentId = StudentId, StatusId = 2 };

            if (booking.Save())
            {
                return CreatedAtAction(nameof(GetBookingById), new { id = booking.BookingId }, new
                {
                    bookingId = booking.BookingId,
                    propertyId = booking.PropertyId,
                    studentId = booking.StudentId,
                    statusId = booking.StatusId,
                    createdAt = booking.CreatedAt,
                    message = "Booking created successfully"
                });
            }

            return Problem(detail: "Error creating booking.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetBookingById(int id)
        {
            if (id <= 0)
                return Problem(detail: "Invalid BookingId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            clsBooking booking = clsBooking.Find(id);
            if (booking == null)
                return Problem(detail: $"Booking {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var property = clsProperties.FindByID(booking.PropertyId);
            bool isPropertyOwner = property != null && property.OwnerID == CurrentUserId;
            if (booking.StudentId != CurrentUserId && !isPropertyOwner && !IsAdmin)
                return Forbid();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            return Ok(new
            {
                bookingId = booking.BookingId,
                statusId = booking.StatusId,
                statusName = booking.BookingStatusInfo?.StatusName,
                createdAt = booking.CreatedAt,
                confirmedAt = booking.ConfirmedAt,
                property = new
                {
                    propertyId = booking.PropertyId,
                    title = booking.PropertyTitle,
                    price = booking.PropertyPrice,
                    address = booking.PropertyAddress,
                    imageUrl = booking.PropertyImagePath == null ? null : $"{baseUrl}/{booking.PropertyImagePath}"
                }
            });
        }

        [HttpGet("student/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetBookingsByStudent(int studentId)
        {
            if (studentId <= 0)
                return Problem(detail: "Invalid StudentId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (studentId != CurrentUserId && !IsAdmin) return Forbid();

            DataTable bookings = clsBooking.GetBookingsByStudentID(studentId);
            if (bookings.Rows.Count == 0)
                return Problem(detail: $"No bookings for student {studentId}.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = bookings.AsEnumerable().Select(row => new
            {
                bookingId = Convert.ToInt32(row["BookingId"]),
                statusId = Convert.ToInt32(row["StatusId"]),
                statusName = row["StatusName"].ToString(),
                createdAt = Convert.ToDateTime(row["CreatedAt"]),
                confirmedAt = row["ConfirmedAt"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["ConfirmedAt"]),
                property = new
                {
                    propertyId = Convert.ToInt32(row["PropertyId"]),
                    title = row["PropertyTitle"].ToString(),
                    price = Convert.ToDecimal(row["Price"]),
                    address = row["PropertyAddress"].ToString(),
                    imageUrl = row["ImagePath"] == DBNull.Value ? null : $"{baseUrl}/{row["ImagePath"]}"
                }
            }).ToList();

            return Ok(result);
        }

        [HttpGet("owner/{ownerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetBookingsByOwner(int ownerId)
        {
            if (ownerId <= 0)
                return Problem(detail: "Invalid OwnerId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (ownerId != CurrentUserId && !IsAdmin) return Forbid();

            DataTable bookings = clsBooking.GetBookingsByOwnerID(ownerId);
            if (bookings.Rows.Count == 0)
                return Problem(detail: $"No bookings for owner {ownerId}.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = bookings.AsEnumerable().Select(row => new
            {
                bookingId = Convert.ToInt32(row["BookingId"]),
                studentName = row["StudentName"].ToString(),
                statusId = Convert.ToInt32(row["StatusId"]),
                statusName = row["StatusName"].ToString(),
                createdAt = Convert.ToDateTime(row["CreatedAt"]),
                confirmedAt = row["ConfirmedAt"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["ConfirmedAt"]),
                property = new
                {
                    propertyId = Convert.ToInt32(row["PropertyId"]),
                    title = row["PropertyTitle"].ToString(),
                    address = row["PropertyAddress"].ToString(),
                    imageUrl = row["ImagePath"] == DBNull.Value ? null : $"{baseUrl}/{row["ImagePath"]}"
                }
            }).ToList();

            return Ok(new { message = "Bookings retrieved successfully", count = result.Count, bookings = result });
        }

        [HttpGet("property/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetBookingsByProperty(int propertyId)
        {
            if (propertyId <= 0)
                return Problem(detail: "Invalid PropertyId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var property = clsProperties.FindByID(propertyId);
            if (property == null)
                return Problem(detail: "Property not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");
            if (property.OwnerID != CurrentUserId && !IsAdmin) return Forbid();

            DataTable bookings = clsBooking.GetBookingsByPropertyID(propertyId);
            if (bookings.Rows.Count == 0)
                return Ok(new { message = "No bookings found", bookings = new List<object>() });

            var result = bookings.AsEnumerable().Select(row => new
            {
                bookingId = Convert.ToInt32(row["BookingId"]),
                studentName = row["StudentName"].ToString(),
                checkInDate = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]).ToString("yyyy-MM-dd") : null,
                statusName = row["StatusName"].ToString()
            }).ToList();

            return Ok(new { message = "Bookings retrieved successfully", count = result.Count, bookings = result });
        }

        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Owner,Admin")]
        public IActionResult ConfirmBooking(int id, int OwnerId)
        {
            if (id <= 0)
                return Problem(detail: "Invalid BookingId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (OwnerId <= 0)
                return Problem(detail: "Invalid OwnerId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (OwnerId != CurrentUserId && !IsAdmin) return Forbid();

            clsBooking booking = clsBooking.Find(id);
            if (booking == null)
                return Problem(detail: $"Booking {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            if (booking.Confirm(OwnerId))
            {
                return Ok(new { bookingId = booking.BookingId, statusId = booking.StatusId, confirmedAt = booking.ConfirmedAt, message = "Booking confirmed successfully." });
            }

            return Problem(detail: "Error confirming booking.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");
        }

        [HttpPut("{id}/cancel")]
        public IActionResult CancelBooking(int id)
        {
            if (id <= 0)
                return Problem(detail: "Invalid BookingId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            clsBooking booking = clsBooking.Find(id);
            if (booking == null)
                return Problem(detail: $"Booking {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            if (booking.StudentId != CurrentUserId && !IsAdmin) return Forbid();

            if (booking.Cancel())
            {
                return Ok(new { bookingId = booking.BookingId, statusId = booking.StatusId, message = "Booking cancelled successfully" });
            }

            return Problem(detail: "Cannot cancel a confirmed or already cancelled booking.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id)
        {
            return Problem(detail: "Use PUT /api/Booking/{id}/cancel instead.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
        }
    }
}