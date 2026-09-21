using Homunity_Buisness_Logic;
using Homunity_Data_Access.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Unit
{
    public class UniversityServiceBuildResponseTests
    {
        [Fact]
        public void BuildResponse_MapsPropertyAndUniversityFieldsCorrectly()
        {
            var property = new PropertyEntity
            {
                PropertyId = 1,
                Title = "Nice Apartment",
                Description = "desc",
                Price = 2000,
                Rooms = 2,
                PropertyType = "Apartment",
                StatusId = 2,
                LocationId = 5,
                Location = new LocationEntity { LocationId = 5, City = "Cairo", Area = "Nasr City" },
                Images = new List<PropertyImageEntity>(),
                Videos = new List<PropertyVideoEntity>(),
                PropertyServices = new List<PropertyServiceEntity>()
            };

            var dto = new PropertyWithDistanceDto
            {
                Property = property,
                UniversityId = 3,
                UniversityName = "Cairo University",
                UniversityLat = 30.0,
                UniversityLon = 31.2,
                DistanceKm = 2.5
            };

            var response = UniversityService.BuildResponse(dto, "https://api.example.com");

            Assert.Equal(1, response.PropertyId);
            Assert.Equal("Cairo University", response.University.UniversityName);
            Assert.Equal(2.5, response.University.DistanceKm);
            Assert.Equal("Cairo", response.Location.City);
        }
    }

}
