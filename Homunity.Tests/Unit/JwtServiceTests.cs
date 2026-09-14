using Homunity_Buisness_Logic;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace Homunity.Tests.Unit
{
    public class JwtServiceTests
    {
        private readonly IConfiguration _configuration;

        public JwtServiceTests()
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsATestJwtSecretKey12345678901234567890",
                ["Jwt:Issuer"] = "HomunityTests",
                ["Jwt:Audience"] = "HomunityTests",
                ["Jwt:ExpiryMinutes"] = "60"
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public void GenerateToken_ReturnsNonEmptyToken_AndFutureExpiry()
        {
            // Arrange
            var jwtService = new JwtService(_configuration);

            // Act
            var result = jwtService.GenerateToken(
                1,
                "01012345678",
                "Owner");

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.True(result.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateToken_ContainsRoleClaim()
        {
            // Arrange
            var jwtService = new JwtService(_configuration);

            // Act
            var result = jwtService.GenerateToken(
                1,
                "01012345678",
                "Owner");

            // Assert
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

            var token = handler.ReadJwtToken(result.Token);

            var roleClaim = token.Claims.FirstOrDefault(
                c => c.Type == System.Security.Claims.ClaimTypes.Role);

            Assert.NotNull(roleClaim);
            Assert.Equal("Owner", roleClaim!.Value);
        }
    }
}