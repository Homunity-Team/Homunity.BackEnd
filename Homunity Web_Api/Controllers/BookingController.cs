using Homunity_Business_Logic;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Booking")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        // =============================================
        // POST: api/Booking
        // Create New Booking (Student Action)
        // =============================================
        [HttpPost]
        public IActionResult CreateBooking(int PropertyId, int StudentId)
        {
            if (PropertyId <= 0)
                return BadRequest(new { message = "Invalid PropertyId" });

            if (StudentId <= 0)
                return BadRequest(new { message = "Invalid StudentId" });

            // StatusId = 2 (InProcess) دايمًا عند الإنشاء - مش المفروض يجي من الـ Request
            clsBooking booking = new clsBooking
            {
                PropertyId = PropertyId,
                StudentId = StudentId,
                StatusId = 2  // InProcess
            };

            if (booking.Save())
            {
                return CreatedAtAction(
                    nameof(GetBookingById),
                    new { id = booking.BookingId },
                    new
                    {
                        bookingId = booking.BookingId,
                        propertyId = booking.PropertyId,
                        studentId = booking.StudentId,
                        statusId = booking.StatusId,
                        createdAt = booking.CreatedAt,
                        message = "Booking created successfully"
                    });
            }

            return StatusCode(500, new
            {
                message = "Error creating booking. Property may already be booked, " +
                          "you may have already requested this property, " +
                          "or you are not a valid student."
            });
        }





        // =============================================
        // GET: api/Booking/{id}
        // =============================================
        [HttpGet("{id}")]
        public IActionResult GetBookingById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid BookingId" });

            clsBooking booking = clsBooking.Find(id);

            if (booking == null)
                return NotFound(new { message = $"Booking with ID {id} not found" });

            return Ok(new
            {
                bookingId = booking.BookingId,
                propertyId = booking.PropertyId,
                studentId = booking.StudentId,
                statusId = booking.StatusId,
                statusName = booking.BookingStatusInfo?.StatusName,
                createdAt = booking.CreatedAt,
                confirmedAt = booking.ConfirmedAt
            });
        }



        // =============================================
        // GET: api/Booking/student/{studentId}
        // =============================================
        [HttpGet("student/{studentId}")]
        public IActionResult GetBookingsByStudent(int studentId)
        {
            if (studentId <= 0)
                return BadRequest(new { message = "Invalid StudentId" });

            DataTable bookings = clsBooking.GetBookingsByStudentID(studentId);

            if (bookings.Rows.Count == 0)
                return NotFound(new { message = $"No bookings found for student {studentId}" });

            return Ok(_ConvertDataTableToList(bookings));
        }

        // =============================================
        // GET: api/Booking/owner/{ownerId}
        // =============================================
        [HttpGet("owner/{ownerId}")]
        public IActionResult GetBookingsByOwner(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest(new { message = "Invalid OwnerId" });

            DataTable bookings = clsBooking.GetBookingsByOwnerID(ownerId);

            if (bookings.Rows.Count == 0)
                return NotFound(new { message = $"No bookings found for owner {ownerId}" });

            return Ok(_ConvertDataTableToList(bookings));
        }

        // =============================================
        // Helper: Convert DataTable → List of Dictionaries
        // =============================================
        private List<Dictionary<string, object>> _ConvertDataTableToList(DataTable dt)
        {
            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }

                result.Add(dict);
            }

            return result;
        }





        // =============================================
        // PUT: api/Booking/{id}/confirm
        // Owner يأكد الحجز → Transaction تشتغل
        // =============================================
        [HttpPut("{id}/confirm")]
        public IActionResult ConfirmBooking(int id, int OwnerId)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid BookingId" });

            if (OwnerId <= 0)
                return BadRequest(new { message = "Invalid OwnerId" });

            clsBooking booking = clsBooking.Find(id);

            if (booking == null)
                return NotFound(new { message = $"Booking with ID {id} not found" });

            if (booking.Confirm(OwnerId))
            {
                return Ok(new
                {
                    bookingId = booking.BookingId,
                    statusId = booking.StatusId,
                    confirmedAt = booking.ConfirmedAt,
                    message = "Booking confirmed successfully. All other bookings for this property have been cancelled."
                });
            }

            return StatusCode(500, new
            {
                message = "Error confirming booking. " +
                          "Booking may already be confirmed, you may not be the owner, " +
                          "or booking is not in InProcess state."
            });
        }





        // =============================================
        // PUT: api/Booking/{id}/cancel
        // =============================================
        [HttpPut("{id}/cancel")]
        public IActionResult CancelBooking(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid BookingId" });

            clsBooking booking = clsBooking.Find(id);

            if (booking == null)
                return NotFound(new { message = $"Booking with ID {id} not found" });

            if (booking.Cancel())
            {
                return Ok(new
                {
                    bookingId = booking.BookingId,
                    statusId = booking.StatusId,
                    message = "Booking cancelled successfully"
                });
            }

            return StatusCode(500, new
            {
                message = "Error cancelling booking. Cannot cancel a confirmed or already cancelled booking."
            });
        }




        // =============================================
        // DELETE: api/Booking/{id}
        // Not Supported - نحول لـ Cancel بدلاً منه
        // =============================================
        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id)
        {
            return BadRequest(new
            {
                message = "Booking deletion is not supported. Use PUT /api/Booking/{id}/cancel instead."
            });
        }
    }
}