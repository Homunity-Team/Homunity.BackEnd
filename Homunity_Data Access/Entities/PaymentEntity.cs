using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Entities
{
    public class PaymentEntity
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public int StudentId { get; set; }
        public int OwnerId { get; set; }
        public int PropertyId { get; set; }
        public decimal Amount { get; set; }
        public string MockOrderId { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public BookingEntity Booking { get; set; } = null!;
    }

}
