using Homunity_Buisness_Logic;
using Homunity_Data_Access.Repositories.Models;
using Homunity_Shared_DTOs.AdminActions;
using Homunity_Web_Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Homunity.Tests.Unit
{
    public class AdminActionsControllerMappingTests
    {
        private static AdminActionsController CreateController(IAdminActionsService service, int adminUserId = 5)
        {
            var controller = new AdminActionsController(service);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, adminUserId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        [Fact]
        public async Task ApproveProperty_Success_ReturnsAdminActionResponseWithApprovedAction()
        {
            var service = new Mock<IAdminActionsService>();
            service.Setup(s => s.GetPropertyDetailAsync(1)).ReturnsAsync(new AdminPropertyDetail { PropertyId = 1, StatusId = 1 });
            service.Setup(s => s.ApproveAsync(1, 5)).ReturnsAsync(true);

            var controller = CreateController(service.Object, adminUserId: 5);
            var result = await controller.ApproveProperty(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AdminActionResponse>(okResult.Value);
            Assert.Equal("Approved", response.Action);
            Assert.Equal(1, response.PropertyId);
            Assert.Equal(5, response.AdminId);
            Assert.Null(response.RejectReason);
        }

        [Fact]
        public async Task RejectProperty_Success_ReturnsAdminActionResponseWithRejectReason()
        {
            var service = new Mock<IAdminActionsService>();
            service.Setup(s => s.GetPropertyDetailAsync(1)).ReturnsAsync(new AdminPropertyDetail { PropertyId = 1, StatusId = 1 });
            service.Setup(s => s.RejectAsync(1, 5, "Not suitable for students")).ReturnsAsync(true);

            var controller = CreateController(service.Object, adminUserId: 5);
            var result = await controller.RejectProperty(1, "Not suitable for students");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AdminActionResponse>(okResult.Value);
            Assert.Equal("Rejected", response.Action);
            Assert.Equal("Not suitable for students", response.RejectReason);
        }

        [Fact]
        public async Task ApproveProperty_PropertyNotPending_ReturnsProblem400()
        {
            var service = new Mock<IAdminActionsService>();
            service.Setup(s => s.GetPropertyDetailAsync(1)).ReturnsAsync(new AdminPropertyDetail { PropertyId = 1, StatusId = 2 });

            var controller = CreateController(service.Object, adminUserId: 5);
            var result = await controller.ApproveProperty(1);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task RejectProperty_PropertyNotFound_ReturnsProblem404()
        {
            var service = new Mock<IAdminActionsService>();
            service.Setup(s => s.GetPropertyDetailAsync(1)).ReturnsAsync((AdminPropertyDetail?)null);

            var controller = CreateController(service.Object, adminUserId: 5);
            var result = await controller.RejectProperty(1, "some valid reason here");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }
    }
}
