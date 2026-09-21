using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Homunity.Tests.Integration
{
    public class AuthIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthIntegrationTests(
            WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_WithMissingCredentials_Returns400()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/Auth/Login",
                new { phone = "", password = "" });

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/Auth/Login",
                new { phone = "01000000000", password = "WrongPassword123" });

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidBody_Returns400WithValidationErrors()
        {
            var client = _factory.CreateClient();

            var content = new StringContent(
                "{}",
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                "/api/Users/Register",
                content);

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.Contains(
                "errors",
                responseBody,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
