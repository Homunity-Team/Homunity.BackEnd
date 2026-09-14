using Homunity_Business_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/BookingStatus")]
    [ApiController]
    [Authorize]
    public class BookingStatusController : AuthorizedControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetAllBookingStatuses()
        {
            DataTable statuses = clsBookingStatus.GetAllBookingStatuses();
            if (statuses.Rows.Count == 0)
                return Problem(detail: "No booking statuses found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(_ConvertDataTableToList(statuses));
        }

        private List<Dictionary<string, object>> _ConvertDataTableToList(DataTable dt)
        {
            var result = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                result.Add(dict);
            }
            return result;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetBookingStatusById(int id)
        {
            if (id <= 0)
                return Problem(detail: "Invalid BookingStatusId.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            clsBookingStatus status = clsBookingStatus.Find(id);
            if (status == null)
                return Problem(detail: $"Booking status with ID {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            return Ok(new { bookingStatusId = status.BookingStatusId, statusName = status.StatusName });
        }
    }
}