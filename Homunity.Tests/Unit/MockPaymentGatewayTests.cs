using Homunity_Buisness_Logic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Homunity.Tests.Unit
{
    public class MockPaymentGatewayTests
    {
        [Fact]
        public async Task ChargeAsync_ValidCard_ReturnsApproved()
        {
            var gateway = new MockPaymentGateway(Mock.Of<ILogger<MockPaymentGateway>>());
            var result = await gateway.ChargeAsync(new PaymentGatewayRequest { OrderId = "HMNT-1-1", CardNumber = "4111111111111111" });

            Assert.True(result.Success);
            Assert.Equal(PaymentGatewayResultStatus.Approved, result.Status);
            Assert.NotNull(result.ProviderTransactionId);
        }

        [Fact]
        public async Task ChargeAsync_InvalidCard_ReturnsDeclined()
        {
            var gateway = new MockPaymentGateway(Mock.Of<ILogger<MockPaymentGateway>>());
            var result = await gateway.ChargeAsync(new PaymentGatewayRequest { OrderId = "HMNT-1-1", CardNumber = "0000000000000000" });

            Assert.False(result.Success);
            Assert.Equal(PaymentGatewayResultStatus.Declined, result.Status);
            Assert.Equal("Invalid card", result.FailureReason);
        }

        [Fact]
        public async Task ChargeAsync_MissingCardNumber_ReturnsDeclined()
        {
            var gateway = new MockPaymentGateway(Mock.Of<ILogger<MockPaymentGateway>>());
            var result = await gateway.ChargeAsync(new PaymentGatewayRequest { OrderId = "HMNT-1-1", CardNumber = "" });

            Assert.False(result.Success);
            Assert.Equal(PaymentGatewayResultStatus.Declined, result.Status);
        }
    }
}