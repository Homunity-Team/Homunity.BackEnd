using Homunity_Shared_DTOs.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Unit
{
    public class BookingResponseModelTests
    {
        [Fact]
        public void BookingListItemResponse_OwnerView_PriceIsNullAndStudentNamePresent()
        {
            var response = new BookingListItemResponse
            {
                BookingId = 1,
                StatusId = 5,
                StudentName = "Ahmed Ali",
                Property = new BookingListPropertyInfo { PropertyId = 10, Title = "Studio", Price = null, Address = "Cairo" }
            };

            Assert.Null(response.Property.Price);
            Assert.Equal("Ahmed Ali", response.StudentName);
        }

        [Fact]
        public void BookingListItemResponse_StudentView_PriceIsPopulatedAndStudentNameIsNull()
        {
            var response = new BookingListItemResponse
            {
                BookingId = 1,
                StatusId = 2,
                StudentName = null,
                Property = new BookingListPropertyInfo { PropertyId = 10, Title = "Studio", Price = 1500m, Address = "Cairo" }
            };

            Assert.Equal(1500m, response.Property.Price);
            Assert.Null(response.StudentName);
        }
    }

}
