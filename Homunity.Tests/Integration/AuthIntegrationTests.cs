using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
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
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync(
                "/api/Auth/Login?phone=&password=",
                null);

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_Returns401()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync(
                "/api/Auth/Login?phone=01000000000&password=WrongPassword123",
                null);

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidBody_Returns400WithValidationErrors()
        {
            // Arrange
            var client = _factory.CreateClient();

            var content = new StringContent(
                "{}",
                System.Text.Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PostAsync(
                "/api/Users/Register",
                content);

            // Assert
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