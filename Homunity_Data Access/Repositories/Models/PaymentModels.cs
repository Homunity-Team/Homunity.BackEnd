using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories.Models
{
    public class BookingForPayment
    {
        public int BookingId { get; set; }
        public int StudentId { get; set; }
        public int PropertyId { get; set; }
        public int OwnerId { get; set; }
        public decimal Price { get; set; }
        public string Title { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string StatusName { get; set; } = "";
        public string? ImageUrl { get; set; }
    }

    public class BookingLockInfo
    {
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int StudentId { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = "";
        public DateTime? ConfirmedAt { get; set; }
    }

    // شكل بسيط لعمود مطابقة الـ raw SQL projection
    public class BookingLockRow
    {
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int StudentId { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = "";
        public DateTime? ConfirmedAt { get; set; }
    }

}
