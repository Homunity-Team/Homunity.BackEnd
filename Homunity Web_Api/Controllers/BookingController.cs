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
            if (PropertyId <= 0) return BadRequest(new { message = "Invalid PropertyId" });
            if (StudentId <= 0) return BadRequest(new { message = "Invalid StudentId" });

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

            return StatusCode(500, new { message = "Error creating booking." });
        }




        // =============================================
        // GET: api/Booking/{id}
        // =============================================
        [HttpGet("{id}")]
        public IActionResult GetBookingById(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid BookingId" });

            clsBooking booking = clsBooking.Find(id);
            if (booking == null) return NotFound(new { message = $"Booking {id} not found" });

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
                    address = booking.PropertyAddress,  // ✅ بدل location
                    imageUrl = booking.PropertyImagePath == null ? null : $"{baseUrl}/{booking.PropertyImagePath}"
                }
            });
        }



        // =============================================
        // GET: api/Booking/student/{studentId}
        // =============================================
        [HttpGet("student/{studentId}")]
        public IActionResult GetBookingsByStudent(int studentId)
        {
            if (studentId <= 0) return BadRequest(new { message = "Invalid StudentId" });

            DataTable bookings = clsBooking.GetBookingsByStudentID(studentId);
            if (bookings.Rows.Count == 0)
                return NotFound(new { message = $"No bookings for student {studentId}" });

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
                    address = row["PropertyAddress"].ToString(),   // ✅ بدل location
                    imageUrl = row["ImagePath"] == DBNull.Value ? null : $"{baseUrl}/{row["ImagePath"]}"
                }
            }).ToList();

            return Ok(result);
        }


        // =============================================
        // GET: api/Booking/owner/{ownerId}
        // =============================================
        [HttpGet("owner/{ownerId}")]
        public IActionResult GetBookingsByOwner(int ownerId)
        {
            if (ownerId <= 0) return BadRequest(new { message = "Invalid OwnerId" });

            DataTable bookings = clsBooking.GetBookingsByOwnerID(ownerId);
            if (bookings.Rows.Count == 0)
                return NotFound(new { message = $"No bookings for owner {ownerId}" });

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
                    address = row["PropertyAddress"].ToString(),   // ✅ بدل location
                    imageUrl = row["ImagePath"] == DBNull.Value ? null : $"{baseUrl}/{row["ImagePath"]}"
                }
            }).ToList();

            return Ok(new
            {
                message = "Bookings retrieved successfully",
                count = result.Count,
                bookings = result
            });
        }



        // =============================================
        // GET: api/Booking/property/{propertyId}
        // =============================================
        [HttpGet("property/{propertyId}")]
        public IActionResult GetBookingsByProperty(int propertyId)
        {
            if (propertyId <= 0) return BadRequest(new { message = "Invalid PropertyId" });

            DataTable bookings = clsBooking.GetBookingsByPropertyID(propertyId);

            if (bookings.Rows.Count == 0)
                return Ok(new { message = "No bookings found", bookings = new List<object>() });

            var result = bookings.AsEnumerable().Select(row => new
            {
                bookingId = Convert.ToInt32(row["BookingId"]),
                studentName = row["StudentName"].ToString(),
                checkInDate = row["CreatedAt"] != DBNull.Value
                              ? Convert.ToDateTime(row["CreatedAt"]).ToString("yyyy-MM-dd") : null,
                statusName = row["StatusName"].ToString()
            }).ToList();

            return Ok(new
            {
                message = "Bookings retrieved successfully",
                count = result.Count,
                bookings = result
            });
        }





        // =============================================
        // PUT: api/Booking/{id}/confirm
        // Owner يأكد الحجز → Transaction تشتغل
        // =============================================
        [HttpPut("{id}/confirm")]
        public IActionResult ConfirmBooking(int id, int OwnerId)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid BookingId" });
            if (OwnerId <= 0) return BadRequest(new { message = "Invalid OwnerId" });

            clsBooking booking = clsBooking.Find(id);
            if (booking == null) return NotFound(new { message = $"Booking {id} not found" });

            if (booking.Confirm(OwnerId))
            {
                return Ok(new
                {
                    bookingId = booking.BookingId,
                    statusId = booking.StatusId,
                    confirmedAt = booking.ConfirmedAt,
                    message = "Booking confirmed successfully."
                });
            }

            return StatusCode(500, new { message = "Error confirming booking." });
        }





        // =============================================
        // PUT: api/Booking/{id}/cancel
        // =============================================
        [HttpPut("{id}/cancel")]
        public IActionResult CancelBooking(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid BookingId" });

            clsBooking booking = clsBooking.Find(id);
            if (booking == null) return NotFound(new { message = $"Booking {id} not found" });

            if (booking.Cancel())
            {
                return Ok(new
                {
                    bookingId = booking.BookingId,
                    statusId = booking.StatusId,
                    message = "Booking cancelled successfully"
                });
            }

            return StatusCode(500, new { message = "Cannot cancel a confirmed or already cancelled booking." });
        }




        // =============================================
        // DELETE: api/Booking/{id}
        // Not Supported - نحول لـ Cancel بدلاً منه
        // =============================================
        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id)
        {
            return BadRequest(new { message = "Use PUT /api/Booking/{id}/cancel instead." });
        }


       
    }
}