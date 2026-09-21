using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.Bookings
{
    public class BookingDetailResponse
    {
        public int BookingId { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public BookingPropertyInfo Property { get; set; }
    }

    public class BookingPropertyInfo
    {
        public int PropertyId { get; set; }
        public string? Title { get; set; }
        public decimal? Price { get; set; }
        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
    }
}
