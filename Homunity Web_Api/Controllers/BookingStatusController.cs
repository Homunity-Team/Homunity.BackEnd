using Homunity_Business_Logic;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/BookingStatus")]
    [ApiController]
    public class BookingStatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllBookingStatuses()
        {
            DataTable statuses = clsBookingStatus.GetAllBookingStatuses();

            if (statuses.Rows.Count == 0)
                return NotFound(new { message = "No booking statuses found" });

            return Ok(_ConvertDataTableToList(statuses));
        }

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
        // GET: api/BookingStatus/{id}
        // للتحقق من Status معين
        // =============================================
        [HttpGet("{id}")]
        public IActionResult GetBookingStatusById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid BookingStatusId" });

            clsBookingStatus status = clsBookingStatus.Find(id);

            if (status == null)
                return NotFound(new { message = $"Booking status with ID {id} not found" });

            return Ok(new
            {
                bookingStatusId = status.BookingStatusId,
                statusName = status.StatusName
            });
        }

         
    }
}