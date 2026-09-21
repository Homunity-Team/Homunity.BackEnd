using Homunity_Buisness_Logic;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories;
using Homunity_Data_Access.Repositories.Models;
using Homunity_Shared_DTOs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Homunity.Tests.Unit
{
    public class PaymentServiceTests
    {
        private const int StudentId = 9;

        private static Mock<IPaymentRepository> NewRepoMock() => new();
        private static Mock<IPaymentGateway> NewGatewayMock() => new();
        private static ILogger<PaymentService> NewLogger() => Mock.Of<ILogger<PaymentService>>();

        private static PaymentEntity PendingPayment(string orderId = "HMNT-1-123", int bookingId = 1, int studentId = StudentId) =>
            new()
            {
                PaymentId = 1,
                BookingId = bookingId,
                StudentId = studentId,
                OwnerId = 3,
                PropertyId = 7,
                Amount = 2000,
                MockOrderId = orderId,
                Status = "Pending"
            };

        [Fact]
        public async Task CreateOrderAsync_BookingNotFound_ReturnsNull()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetBookingForPaymentAsync(It.IsAny<int>())).ReturnsAsync((BookingForPayment?)null);

            var service = new PaymentService(repo.Object, NewGatewayMock().Object, NewLogger());
            var result = await service.CreateOrderAsync(1, 2);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateOrderAsync_BookingNotConfirmed_ReturnsNull()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetBookingForPaymentAsync(It.IsAny<int>()))
                .ReturnsAsync(new BookingForPayment { StatusName = "InProcess", StudentId = 2 });

            var service = new PaymentService(repo.Object, NewGatewayMock().Object, NewLogger());
            var result = await service.CreateOrderAsync(1, 2);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateOrderAsync_WrongStudent_ReturnsNull()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetBookingForPaymentAsync(It.IsAny<int>()))
                .ReturnsAsync(new BookingForPayment { StatusName = "Confirmed", StudentId = 99, Price = 1000 });

            var service = new PaymentService(repo.Object, NewGatewayMock().Object, NewLogger());
            var result = await service.CreateOrderAsync(1, 2);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateOrderAsync_DuplicatePendingPayment_ReturnsNull()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetBookingForPaymentAsync(It.IsAny<int>()))
                .ReturnsAsync(new BookingForPayment { StatusName = "Confirmed", Price = 1000, StudentId = 2 });
            repo.Setup(r => r.HasPendingPaymentAsync(It.IsAny<int>())).ReturnsAsync(true);

            var service = new PaymentService(repo.Object, NewGatewayMock().Object, NewLogger());
            var result = await service.CreateOrderAsync(1, 2);

            Assert.Null(result);
            repo.Verify(r => r.CreatePaymentAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_Success_CalculatesAmountAsDoublePrice()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetBookingForPaymentAsync(5))
                .ReturnsAsync(new BookingForPayment { BookingId = 5, StatusName = "Confirmed", Price = 1000, OwnerId = 3, PropertyId = 7, StudentId = 9, Title = "Studio" });
            repo.Setup(r => r.HasPendingPaymentAsync(5)).ReturnsAsync(false);

            var service = new PaymentService(repo.Object, NewGatewayMock().Object, NewLogger());
            var result = await service.CreateOrderAsync(5, 9);

            Assert.NotNull(result);
            Assert.Equal(2000, result!.Amount);
            Assert.Equal(5, result.BookingId);
        }

        [Fact]
        public async Task ProcessPaymentAsync_NullRequest_ReturnsInvalidRequest()
        {
            var service = new PaymentService(NewRepoMock().Object, NewGatewayMock().Object, NewLogger());
            var (success, message) = await service.ProcessPaymentAsync(null!, StudentId);

            Assert.False(success);
            Assert.Equal("Invalid request", message);
        }

        [Fact]
        public async Task ProcessPaymentAsync_PaymentNotFound_ReturnsInvalidOrder()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetPaymentByMockOrderIdAsync(It.IsAny<string>())).ReturnsAsync((PaymentEntity?)null);

            var service = new PaymentService(repo.Object, NewGatewayMock().Object, NewLogger());
            var (success, message) = await service.ProcessPaymentAsync(
                new MockProcessRequest { MockOrderId = "INVALID", CardNumber = "4111111111111111" },
                StudentId);

            Assert.False(success);
            Assert.Equal("Invalid order", message);
        }

        [Fact]
        public async Task ProcessPaymentAsync_WrongStudent_ReturnsForbidden()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetPaymentByMockOrderIdAsync("HMNT-1-123"))
                .ReturnsAsync(PendingPayment(studentId: 99));

            var service = new PaymentService(repo.Object, NewGatewayMock().Object, NewLogger());
            var (success, message) = await service.ProcessPaymentAsync(
                new MockProcessRequest { MockOrderId = "HMNT-1-123", CardNumber = "4111111111111111" },
                StudentId);

            Assert.False(success);
            Assert.Equal("Forbidden", message);
        }

        [Fact]
        public async Task ProcessPaymentAsync_GatewayDeclines_ReturnsGatewayFailureReason()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetPaymentByMockOrderIdAsync("HMNT-1-123")).ReturnsAsync(PendingPayment());
            repo.Setup(r => r.GetBookingForPaymentWithLockAsync(1))
                .ReturnsAsync(new BookingLockInfo { BookingId = 1, StatusName = "Confirmed" });

            var gateway = NewGatewayMock();
            gateway.Setup(g => g.ChargeAsync(It.IsAny<PaymentGatewayRequest>()))
                .ReturnsAsync(PaymentGatewayResult.Declined("Invalid card"));

            var service = new PaymentService(repo.Object, gateway.Object, NewLogger());
            var (success, message) = await service.ProcessPaymentAsync(
                new MockProcessRequest { MockOrderId = "HMNT-1-123", CardNumber = "0000000000000000" },
                StudentId);

            Assert.False(success);
            Assert.Equal("Invalid card", message);
        }

        [Fact]
        public async Task ProcessPaymentAsync_BookingNotConfirmed_ReturnsFailure_WithoutCharging()
        {
            var repo = NewRepoMock();
            repo.Setup(r => r.GetPaymentByMockOrderIdAsync("HMNT-1-123")).ReturnsAsync(PendingPayment());
            repo.Setup(r => r.GetBookingForPaymentWithLockAsync(1))
                .ReturnsAsync(new BookingLockInfo { BookingId = 1, StatusName = "InProcess" });

            var gateway = NewGatewayMock();

            var service = new PaymentService(repo.Object, gateway.Object, NewLogger());
            var (success, message) = await service.ProcessPaymentAsync(
                new MockProcessRequest { MockOrderId = "HMNT-1-123", CardNumber = "4111111111111111" },
                StudentId);

            Assert.False(success);
            Assert.Equal("Booking not confirmed", message);
            gateway.Verify(g => g.ChargeAsync(It.IsAny<PaymentGatewayRequest>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Success_UpdatesPaymentAndBookingStatus()
        {
            var gateway = NewGatewayMock();
            gateway.Setup(g => g.ChargeAsync(It.IsAny<PaymentGatewayRequest>()))
                .ReturnsAsync(PaymentGatewayResult.Approved("TXN-1"));

            var repo = NewRepoMock();
            repo.Setup(r => r.GetPaymentByMockOrderIdAsync("HMNT-1-123")).ReturnsAsync(PendingPayment());
            repo.Setup(r => r.GetBookingForPaymentWithLockAsync(1))
                .ReturnsAsync(new BookingLockInfo { BookingId = 1, StatusName = "Confirmed" });
            repo.Setup(r => r.UpdatePaymentStatusAsync("HMNT-1-123", "Success")).ReturnsAsync(true);
            repo.Setup(r => r.UpdateBookingStatusToBookedAsync(1)).ReturnsAsync(true);

            var service = new PaymentService(repo.Object, gateway.Object, NewLogger());
            var (success, message) = await service.ProcessPaymentAsync(
                new MockProcessRequest { MockOrderId = "HMNT-1-123", CardNumber = "4111111111111111" },
                StudentId);

            Assert.True(success);
            Assert.Equal("Payment completed successfully", message);
            repo.Verify(r => r.UpdatePaymentStatusAsync("HMNT-1-123", "Success"), Times.Once);
            repo.Verify(r => r.UpdateBookingStatusToBookedAsync(1), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentAsync_BookingUpdateFails_RollsBackPaymentToFailed()
        {
            var gateway = NewGatewayMock();
            gateway.Setup(g => g.ChargeAsync(It.IsAny<PaymentGatewayRequest>()))
                .ReturnsAsync(PaymentGatewayResult.Approved("TXN-1"));

            var repo = NewRepoMock();
            repo.Setup(r => r.GetPaymentByMockOrderIdAsync("HMNT-1-123")).ReturnsAsync(PendingPayment());
            repo.Setup(r => r.GetBookingForPaymentWithLockAsync(1))
                .ReturnsAsync(new BookingLockInfo { BookingId = 1, StatusName = "Confirmed" });
            repo.Setup(r => r.UpdatePaymentStatusAsync("HMNT-1-123", "Success")).ReturnsAsync(true);
            repo.Setup(r => r.UpdateBookingStatusToBookedAsync(1)).ReturnsAsync(false);

            var service = new PaymentService(repo.Object, gateway.Object, NewLogger());
            var (success, message) = await service.ProcessPaymentAsync(
                new MockProcessRequest { MockOrderId = "HMNT-1-123", CardNumber = "4111111111111111" },
                StudentId);

            Assert.False(success);
            Assert.Equal("Booking update failed", message);
            repo.Verify(r => r.UpdatePaymentStatusAsync("HMNT-1-123", "Failed"), Times.Once);
        }
    }
}
