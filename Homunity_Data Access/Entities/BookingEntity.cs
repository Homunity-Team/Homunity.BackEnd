using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Entities
{
    public class BookingEntity
    {
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int StudentId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        public PropertyEntity Property { get; set; } = null!;
        public UserEntity Student { get; set; } = null!;
        public BookingStatusEntity Status { get; set; } = null!;
        public List<PaymentEntity> Payments { get; set; } = new();

    }
}
