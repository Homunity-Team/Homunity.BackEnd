using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Homunity.Tests.Integration
{
    public class PropertiesHappyPathTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PropertiesHappyPathTests(WebApplicationFactory<Program> factory)
        {
            var customFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    var testSettings = new Dictionary<string, string?>
                    {
                        { "Jwt:Key", "Homunity-Super-Secret-Dev-Key-Change-Me-32Chars!" },
                        { "Jwt:Issuer", "HomunityAPI" },
                        { "Jwt:Audience", "HomunityClient" },
                        { "Jwt:ExpiryMinutes", "60" },
                        { "ConnectionStrings:HomunityDb", "Server=localhost;Database=Homunity;User Id=sa;Password=123456;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;" },
                        { "Cors:AllowedOrigins:0", "https://localhost:7089" }
                    };
                    config.AddInMemoryCollection(testSettings);
                });
            });

            _client = customFactory.CreateClient();
        }

        [Fact]
        public async Task RegisterLoginAndReadProperties_FullFlow_Succeeds()
        {
            var uniquePhone = $"010{DateTime.UtcNow.Ticks % 100000000}";

            var registerRequest = new
            {
                firstName = "Test",
                lastName = "Owner",
                phone = uniquePhone,
                password = "TestPass123",
                roleId = 2
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/Users/Register", registerRequest);
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Auth/Login",
                new { phone = uniquePhone, password = "TestPass123" });
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginBody = await loginResponse.Content.ReadAsStringAsync();
            using var loginJson = JsonDocument.Parse(loginBody);

            var token = loginJson.RootElement.GetProperty("token").GetString();
            Assert.False(string.IsNullOrWhiteSpace(token));

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var getAllResponse = await _client.GetAsync("/api/Properties/GetAllV2");
            Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

            var getAllBody = await getAllResponse.Content.ReadAsStringAsync();
            using var pagedJson = JsonDocument.Parse(getAllBody);

            Assert.True(pagedJson.RootElement.TryGetProperty("pageNumber", out _));
            Assert.True(pagedJson.RootElement.TryGetProperty("totalCount", out _));
            Assert.True(pagedJson.RootElement.TryGetProperty("items", out _));
        }
    }
}