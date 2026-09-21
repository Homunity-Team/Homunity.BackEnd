using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Integration
{
    // Requires the same real local SQL Server instance the other integration tests in this
    // project already require (see Homunity.Tests/README.md). EF Core's InMemory provider
    // does NOT support ExecuteDeleteAsync, so this cascade-delete logic cannot be verified
    // against an in-memory database — a relational provider is mandatory here.
    public class PropertyDeleteIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PropertyDeleteIntegrationTests(WebApplicationFactory<Program> factory)
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
        public async Task DeleteProperty_WithoutToken_Returns401()
        {
            var response = await _client.DeleteAsync("/api/Properties/DeleteProperty?id=1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // NOTE: A full end-to-end "register owner -> create property with images -> delete ->
        // assert images/services/booking rows are gone" test requires seeding a complete
        // property graph through the multipart CreateFullProperty endpoint first. I have not
        // written that full seeding flow here because it depends on test image files and
        // exact seed data conventions I don't have visibility into from the codebase alone.
        // This is flagged as a gap in section I below, not silently skipped.
    }
}
