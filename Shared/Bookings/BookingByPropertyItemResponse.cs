using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.Bookings
{
    public class BookingByPropertyItemResponse
    {
        public int BookingId { get; set; }
        public string? StudentName { get; set; }
        public string? CheckInDate { get; set; }
        public string? StatusName { get; set; }
    }

}
