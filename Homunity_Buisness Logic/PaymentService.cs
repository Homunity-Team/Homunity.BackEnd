using System;
using System.Threading.Tasks;
using Homunity_Data_Access.Repositories;
using Homunity_Shared_DTOs;
using Microsoft.Extensions.Logging;

namespace Homunity_Buisness_Logic
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly IPaymentGateway _gateway;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentRepository repo, IPaymentGateway gateway, ILogger<PaymentService> logger)
        {
            _repo = repo;
            _gateway = gateway;
            _logger = logger;
        }

        public async Task<PaymentOrderResponse?> CreateOrderAsync(int bookingId, int studentId)
        {
            var booking = await _repo.GetBookingForPaymentAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("CreateOrder rejected: booking {BookingId} not found.", bookingId);
                return null;
            }

            // Ownership: only the student on the booking may create a payment order
            if (booking.StudentId != studentId)
            {
                _logger.LogWarning("CreateOrder rejected: student {StudentId} does not own booking {BookingId}.", studentId, bookingId);
                return null;
            }

            if (!booking.StatusName.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("CreateOrder rejected: booking {BookingId} is not Confirmed (status: {Status}).", bookingId, booking.StatusName);
                return null;
            }

            if (await _repo.HasPendingPaymentAsync(bookingId))
            {
                _logger.LogWarning("CreateOrder rejected: duplicate/pending payment already exists for booking {BookingId}.", bookingId);
                return null;
            }

            decimal amount = booking.Price * 2;
            string mockOrderId = $"HMNT-{bookingId}-{DateTime.UtcNow.Ticks}";

            await _repo.CreatePaymentAsync(bookingId, studentId, booking.OwnerId, booking.PropertyId, amount, mockOrderId);

            _logger.LogInformation("CreateOrder succeeded: order {OrderId} created for booking {BookingId}, amount {Amount}.", mockOrderId, bookingId, amount);

            return new PaymentOrderResponse
            {
                MockOrderId = mockOrderId,
                BookingId = bookingId,
                PropertyId = booking.PropertyId,
                PropertyTitle = booking.Title,
                PropertyImage = booking.ImageUrl,
                Amount = amount,
                StudentName = booking.StudentName,
                Status = "created"
            };
        }

        public async Task<(bool success, string message)> ProcessPaymentAsync(MockProcessRequest req, int actingStudentId)
        {
            if (req == null || string.IsNullOrEmpty(req.MockOrderId))
            {
                _logger.LogWarning("ProcessPayment rejected: missing request or MockOrderId.");
                return (false, "Invalid request");
            }

            var payment = await _repo.GetPaymentByMockOrderIdAsync(req.MockOrderId);
            if (payment == null)
            {
                _logger.LogWarning("ProcessPayment rejected: payment not found for order {OrderId}.", req.MockOrderId);
                return (false, "Invalid order");
            }

            if (!string.Equals(payment.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("ProcessPayment rejected: payment {OrderId} is not Pending (status: {Status}).", req.MockOrderId, payment.Status);
                return (false, "Payment is not pending");
            }

            if (payment.StudentId != actingStudentId)
            {
                _logger.LogWarning("ProcessPayment rejected: student {StudentId} does not own payment order {OrderId}.", actingStudentId, req.MockOrderId);
                return (false, "Forbidden");
            }

            var booking = await _repo.GetBookingForPaymentWithLockAsync(payment.BookingId);
            if (booking == null)
            {
                _logger.LogWarning("ProcessPayment rejected: booking {BookingId} not found.", payment.BookingId);
                return (false, "Booking not found");
            }

            if (!booking.StatusName.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("ProcessPayment rejected: booking {BookingId} not confirmed (status: {Status}).", payment.BookingId, booking.StatusName);
                return (false, "Booking not confirmed");
            }

            // Charge gateway ONLY after all validations pass
            var gatewayResult = await _gateway.ChargeAsync(new PaymentGatewayRequest
            {
                OrderId = req.MockOrderId,
                Amount = payment.Amount,
                CardNumber = req.CardNumber,
                CardExpiry = req.CardExpiry,
                CardCvv = req.CardCvv
            });

            if (!gatewayResult.Success)
            {
                _logger.LogWarning("ProcessPayment declined by gateway for order {OrderId}: {Reason}", req.MockOrderId, gatewayResult.FailureReason);
                return (false, gatewayResult.FailureReason ?? "Invalid card");
            }

            bool paymentOk = await _repo.UpdatePaymentStatusAsync(req.MockOrderId, "Success");
            if (!paymentOk)
            {
                _logger.LogError("ProcessPayment: gateway approved but local payment update failed for order {OrderId}.", req.MockOrderId);
                return (false, "Payment update failed");
            }

            bool bookingOk = await _repo.UpdateBookingStatusToBookedAsync(payment.BookingId);
            if (!bookingOk)
            {
                await _repo.UpdatePaymentStatusAsync(req.MockOrderId, "Failed");
                _logger.LogError("ProcessPayment: booking update to Booked failed after successful payment for order {OrderId}; payment marked Failed.", req.MockOrderId);
                return (false, "Booking update failed");
            }

            _logger.LogInformation("ProcessPayment succeeded for order {OrderId}, booking {BookingId}.", req.MockOrderId, payment.BookingId);
            return (true, "Payment completed successfully");
        }

        public async Task<PaymentStatusResponse?> GetStatusAsync(int bookingId, int actingUserId, bool isOwnerOrAdmin)
        {
            var booking = await _repo.GetBookingForPaymentAsync(bookingId);
            if (booking == null) return null;

            // Student on the booking, property owner, or any Admin
            bool allowed =
                booking.StudentId == actingUserId
                || booking.OwnerId == actingUserId
                || isOwnerOrAdmin;
            if (!allowed)
            {
                _logger.LogWarning("GetStatus forbidden for user {UserId} on booking {BookingId}.", actingUserId, bookingId);
                return null;
            }

            var payment = await _repo.GetPaymentByBookingIdAsync(bookingId);

            return new PaymentStatusResponse
            {
                BookingId = bookingId,
                PaymentStatus = payment?.Status ?? "NotCreated",
                BookingStatus = booking.StatusName,
                Amount = payment?.Amount ?? 0,
                PaidAt = payment?.PaidAt != null ? payment.PaidAt.Value.ToString("yyyy-MM-dd HH:mm") : null
            };
        }
    }
}
