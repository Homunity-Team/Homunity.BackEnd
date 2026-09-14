using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Homunity.Tests.Integration
{
    public class PropertiesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PropertiesIntegrationTests(WebApplicationFactory<Program> factory)
        {
            var customFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // نفس القيم الحقيقية المسجّلة في User Secrets (Sprint 4) —
                    // مكتوبة هنا صراحة عشان نضمن توفرها في بيئة WebApplicationFactory الخاصة بالاختبارات.
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
        public async Task GetAllV2_WithoutToken_Returns401()
        {
            var response = await _client.GetAsync("/api/Properties/GetAllV2");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetByIDV2_WithoutToken_Returns401()
        {
            var response = await _client.GetAsync("/api/Properties/GetByIDV2?id=1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateFullProperty_WithoutToken_Returns401()
        {
            using var content = new MultipartFormDataContent();
            var response = await _client.PostAsync("/api/Properties/CreateFullProperty", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}