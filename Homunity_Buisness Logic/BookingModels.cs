using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class BookingCreateResult
    {
        public bool Success { get; set; }
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int StudentId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }

        public static BookingCreateResult Fail() => new() { Success = false };
    }

    public class BookingActionResult
    {
        public bool Success { get; set; }
        public bool NotFoundFlag { get; set; }
        public int BookingId { get; set; }
        public int StatusId { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        public static BookingActionResult Fail() => new() { Success = false };
        public static BookingActionResult NotFound() => new() { Success = false, NotFoundFlag = true };
        public static BookingActionResult Ok(int bookingId, int statusId, DateTime? confirmedAt) =>
            new() { Success = true, BookingId = bookingId, StatusId = statusId, ConfirmedAt = confirmedAt };
    }
}
