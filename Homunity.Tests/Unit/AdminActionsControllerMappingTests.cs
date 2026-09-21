using Homunity_Buisness_Logic;
using Homunity_Data_Access.Repositories.Models;
using Homunity_Shared_DTOs.AdminActions;
using Homunity_Web_Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Unit
{
    public class AdminActionsControllerMappingTests
    {
        [Fact]
        public async Task ApproveProperty_Success_ReturnsAdminActionResponseWithApprovedAction()
        {
            var service = new Mock<IAdminActionsService>();
            service.Setup(s => s.GetPropertyDetailAsync(1)).ReturnsAsync(new AdminPropertyDetail { PropertyId = 1, StatusId = 1 });
            service.Setup(s => s.ApproveAsync(1, 5)).ReturnsAsync(true);

            var controller = new AdminActionsController(service.Object);
            var result = await controller.ApproveProperty(1, 5);

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

            var controller = new AdminActionsController(service.Object);
            var result = await controller.RejectProperty(1, 5, "Not suitable for students");

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

            var controller = new AdminActionsController(service.Object);
            var result = await controller.ApproveProperty(1, 5);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task RejectProperty_PropertyNotFound_ReturnsProblem404()
        {
            var service = new Mock<IAdminActionsService>();
            service.Setup(s => s.GetPropertyDetailAsync(1)).ReturnsAsync((AdminPropertyDetail?)null);

            var controller = new AdminActionsController(service.Object);
            var result = await controller.RejectProperty(1, 5, "some valid reason here");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }
    }
}
