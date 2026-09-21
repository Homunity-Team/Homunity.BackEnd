using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.Bookings
{
    public class BookingListItemResponse
    {
        public int BookingId { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        // يظهر فقط في سياق "حجوزات المالك" (GetBookingsByOwner)، ويبقى null في سياق الطالب
        public string? StudentName { get; set; }

        public BookingListPropertyInfo Property { get; set; }
    }

    public class BookingListPropertyInfo
    {
        public int PropertyId { get; set; }
        public string? Title { get; set; }

        // يظهر فقط في سياق حجوزات الطالب. في سياق المالك يبقى null — هذا مطابق تمامًا
        // للسلوك الأصلي (الحقل لم يكن موجودًا إطلاقًا في استجابة GetBookingsByOwner الأصلية)
        public decimal? Price { get; set; }

        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
    }

}
