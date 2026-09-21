using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Integration
{
    public class AuthorizationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public AuthorizationIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task GetBookingsByStudent_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Booking/student/1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ChatHistory_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Chat/history/1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetUserProfile_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Users/Get%20Profile%20By%20ID?id=1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

}
