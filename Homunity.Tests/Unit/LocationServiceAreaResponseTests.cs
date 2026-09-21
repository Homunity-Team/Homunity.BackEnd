using Homunity_Buisness_Logic;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Unit
{
    public class LocationServiceAreaResponseTests
    {
        [Fact]
        public async Task GetAreasByCityAsync_MapsToAreaResponse()
        {
            var locationRepo = new Mock<ILocationRepository>();
            locationRepo.Setup(r => r.GetAreasByCityAsync("Cairo")).ReturnsAsync(new List<LocationEntity>
            {
                new LocationEntity { LocationId = 1, City = "Cairo", Area = "Nasr City", Street = "St 1", Latitude = 30.0, Longitude = 31.2 }
            });

            var universityRepo = new Mock<IUniversityRepository>();
            var propertyRepo = new Mock<IPropertyRepository>();

            var service = new LocationService(locationRepo.Object, universityRepo.Object, propertyRepo.Object);
            var result = await service.GetAreasByCityAsync("Cairo");

            Assert.Single(result);
            Assert.Equal("Nasr City", result[0].Area);
            Assert.Equal(1, result[0].LocationId);
        }
    }

}
