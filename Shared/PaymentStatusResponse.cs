using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs
{
    public class PaymentStatusResponse
    {
        public int BookingId { get; set; }
        public string PaymentStatus { get; set; }  // Pending/Success/Failed
        public string BookingStatus { get; set; }  // Confirmed/Booked
        public decimal Amount { get; set; }
        public string PaidAt { get; set; }
    }
}
