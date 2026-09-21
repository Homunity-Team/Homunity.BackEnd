using Homunity_Buisness_Logic;
using Homunity_Data_Access.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Unit
{
    public class PropertyServiceTests
    {
        private static Mock<IPropertyRepository> NewRepoMock() => new();

        [Fact]
        public async Task GetOwnershipAsync_PropertyNotFound_ReturnsNull()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetOwnershipAsync(It.IsAny<int>())).ReturnsAsync(((int, int)?)null);
            var service = new PropertyService(repo.Object, Mock.Of<ILogger<PropertyService>>());

            var result = await service.GetOwnershipAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task DeletePropertyAsync_PropertyNotFound_ReturnsNotFound()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetOwnershipAsync(It.IsAny<int>())).ReturnsAsync(((int, int)?)null);
            var service = new PropertyService(repo.Object, Mock.Of<ILogger<PropertyService>>());

            var outcome = await service.DeletePropertyAsync(1, 5, false);

            Assert.Equal(PropertyDeleteStatus.NotFound, outcome.Status);
            repo.Verify(r => r.DeletePropertyCascadeAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeletePropertyAsync_NotOwnerNotAdmin_ReturnsForbidden()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetOwnershipAsync(It.IsAny<int>())).ReturnsAsync((OwnerId: 99, LocationId: 1));
            var service = new PropertyService(repo.Object, Mock.Of<ILogger<PropertyService>>());

            var outcome = await service.DeletePropertyAsync(1, 5, false);

            Assert.Equal(PropertyDeleteStatus.Forbidden, outcome.Status);
            repo.Verify(r => r.DeletePropertyCascadeAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeletePropertyAsync_Admin_CanDeleteEvenIfNotOwner()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetOwnershipAsync(It.IsAny<int>())).ReturnsAsync((OwnerId: 99, LocationId: 1));
            repo.Setup(r => r.DeletePropertyCascadeAsync(It.IsAny<int>())).ReturnsAsync(true);
            var service = new PropertyService(repo.Object, Mock.Of<ILogger<PropertyService>>());

            var outcome = await service.DeletePropertyAsync(1, 5, isAdmin: true);

            Assert.Equal(PropertyDeleteStatus.Success, outcome.Status);
        }

        [Fact]
        public async Task DeletePropertyAsync_OwnerMatches_CallsCascadeDelete_ReturnsSuccess()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetOwnershipAsync(1)).ReturnsAsync((OwnerId: 5, LocationId: 1));
            repo.Setup(r => r.DeletePropertyCascadeAsync(1)).ReturnsAsync(true);
            var service = new PropertyService(repo.Object, Mock.Of<ILogger<PropertyService>>());

            var outcome = await service.DeletePropertyAsync(1, 5, false);

            Assert.Equal(PropertyDeleteStatus.Success, outcome.Status);
        }

        [Fact]
        public async Task DeletePropertyAsync_RepositoryFails_ReturnsFailed()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetOwnershipAsync(1)).ReturnsAsync((OwnerId: 5, LocationId: 1));
            repo.Setup(r => r.DeletePropertyCascadeAsync(1)).ReturnsAsync(false);
            var service = new PropertyService(repo.Object, Mock.Of<ILogger<PropertyService>>());

            var outcome = await service.DeletePropertyAsync(1, 5, false);

            Assert.Equal(PropertyDeleteStatus.Failed, outcome.Status);
        }
    }
}
