using Homunity_Buisness_Logic;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories;
using Moq;
using Xunit;

namespace Homunity.Tests.Unit
{
    public class UniversityDistanceSearchTests
    {
        [Fact]
        public void CalculateDistance_SamePoint_IsZero()
        {
            var d = UniversityService.CalculateDistance(30.0, 31.0, 30.0, 31.0);
            Assert.Equal(0, d);
        }

        [Fact]
        public void CalculateDistance_KnownSeparation_IsPositiveAndRounded()
        {
            // Roughly ~111 km per 1 degree latitude
            var d = UniversityService.CalculateDistance(30.0, 31.0, 31.0, 31.0);
            Assert.True(d > 100 && d < 120, $"Expected ~111km, got {d}");
        }

        [Fact]
        public async Task SearchByUniversityAsync_FiltersByMaxDistanceKm()
        {
            var uni = new UniversityEntity
            {
                UniversityId = 1,
                Name = "Test Uni",
                Latitude = 30.0,
                Longitude = 31.0
            };

            var near = new PropertyEntity
            {
                PropertyId = 1,
                Title = "Near",
                Price = 1000,
                StatusId = 2,
                UniversityId = 1,
                Location = new LocationEntity { Latitude = 30.01, Longitude = 31.0 }
            };
            var far = new PropertyEntity
            {
                PropertyId = 2,
                Title = "Far",
                Price = 1000,
                StatusId = 2,
                UniversityId = 1,
                Location = new LocationEntity { Latitude = 31.5, Longitude = 31.0 }
            };

            var repo = new Mock<IUniversityRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(uni);
            repo.Setup(r => r.SearchByUniversityIdAsync(1, null))
                .ReturnsAsync(new List<PropertyEntity> { near, far });

            var service = new UniversityService(repo.Object);
            var results = await service.SearchByUniversityAsync(1, maxPrice: null, maxDistanceKm: 20);

            Assert.Single(results);
            Assert.Equal(1, results[0].Property.PropertyId);
            Assert.NotNull(results[0].DistanceKm);
            Assert.True(results[0].DistanceKm < 20);
        }

        [Fact]
        public async Task SearchByUniversityAsync_OrdersNearestFirst()
        {
            var uni = new UniversityEntity
            {
                UniversityId = 1,
                Name = "Test Uni",
                Latitude = 30.0,
                Longitude = 31.0
            };

            var closer = new PropertyEntity
            {
                PropertyId = 10,
                Title = "Closer",
                StatusId = 2,
                UniversityId = 1,
                Location = new LocationEntity { Latitude = 30.02, Longitude = 31.0 }
            };
            var farther = new PropertyEntity
            {
                PropertyId = 11,
                Title = "Farther",
                StatusId = 2,
                UniversityId = 1,
                Location = new LocationEntity { Latitude = 30.08, Longitude = 31.0 }
            };

            var repo = new Mock<IUniversityRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(uni);
            repo.Setup(r => r.SearchByUniversityIdAsync(1, null))
                .ReturnsAsync(new List<PropertyEntity> { farther, closer });

            var service = new UniversityService(repo.Object);
            var results = await service.SearchByUniversityAsync(1, null, null);

            Assert.Equal(2, results.Count);
            Assert.Equal(10, results[0].Property.PropertyId);
            Assert.True(results[0].DistanceKm <= results[1].DistanceKm);
        }
    }
}
