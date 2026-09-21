using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Integration
{
    // اختبارات دخان (smoke) للتأكد إن تغييرات الـDTOs في Sprint 3 ما كسرتش الـrouting/الـattributes.
    public class Sprint3ResponseContractSmokeTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public Sprint3ResponseContractSmokeTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task ApproveProperty_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PutAsync("/api/AdminActions/properties/1/approve?adminId=1", null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RolesEndpoint_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Roles/Get%20All%20Roles");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ServicesEndpoint_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Services/GetAll");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

}
