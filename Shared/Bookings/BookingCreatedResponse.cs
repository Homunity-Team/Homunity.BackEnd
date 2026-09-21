using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.Bookings
{
    public class BookingCreatedResponse
    {
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int StudentId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; }
    }

}
