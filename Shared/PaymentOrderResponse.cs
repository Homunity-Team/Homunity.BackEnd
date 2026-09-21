using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs
{
    public class PaymentOrderResponse
    {
        public string MockOrderId { get; set; }
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public string PropertyImage { get; set; }
        public decimal Amount { get; set; }
        public string StudentName { get; set; }
        public string Status { get; set; }  // "created"
    }
}
