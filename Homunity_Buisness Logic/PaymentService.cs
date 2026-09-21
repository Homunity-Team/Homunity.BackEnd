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

            if (!booking.StatusName.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("CreateOrder rejected: booking {BookingId} is not Confirmed (status: {Status}).", bookingId, booking.StatusName);
                return null;
            }

            if (await _repo.HasPendingPaymentAsync(bookingId))
            {
                // سيناريو فشل متوقَّع: محاولة دفع مكررة على نفس الحجز خلال نافذة زمنية قصيرة.
                _logger.LogWarning("CreateOrder rejected: duplicate/pending payment already exists for booking {BookingId}.", bookingId);
                return null;
            }

            decimal amount = booking.Price * 2;
            string mockOrderId = $"HMNT-{bookingId}-{DateTime.Now.Ticks}";

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

        public async Task<(bool success, string message)> ProcessPaymentAsync(MockProcessRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.MockOrderId))
            {
                _logger.LogWarning("ProcessPayment rejected: missing request or MockOrderId.");
                return (false, "Invalid request");
            }

            // البوابة (mock حاليًا، قابلة للاستبدال بمزوّد حقيقي دون أي تغيير في هذه الميثود)
            var gatewayResult = await _gateway.ChargeAsync(new PaymentGatewayRequest
            {
                OrderId = req.MockOrderId,
                CardNumber = req.CardNumber,
                CardExpiry = req.CardExpiry,
                CardCvv = req.CardCvv
            });

            if (!gatewayResult.Success)
            {
                _logger.LogWarning("ProcessPayment declined by gateway for order {OrderId}: {Reason}", req.MockOrderId, gatewayResult.FailureReason);
                return (false, gatewayResult.FailureReason ?? "Invalid card");
            }

            var parts = req.MockOrderId.Split('-');
            if (parts.Length < 2 || !int.TryParse(parts[1], out int bookingId))
            {
                _logger.LogWarning("ProcessPayment rejected: malformed MockOrderId {OrderId}.", req.MockOrderId);
                return (false, "Invalid order");
            }

            var booking = await _repo.GetBookingForPaymentWithLockAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("ProcessPayment rejected: booking {BookingId} not found.", bookingId);
                return (false, "Booking not found");
            }

            if (!booking.StatusName.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("ProcessPayment rejected: booking {BookingId} not confirmed (status: {Status}).", bookingId, booking.StatusName);
                return (false, "Booking not confirmed");
            }

            bool paymentOk = await _repo.UpdatePaymentStatusAsync(req.MockOrderId, "Success");
            if (!paymentOk)
            {
                _logger.LogError("ProcessPayment: gateway approved but local payment update failed for order {OrderId}.", req.MockOrderId);
                return (false, "Payment update failed");
            }

            bool bookingOk = await _repo.UpdateBookingStatusToBookedAsync(bookingId);
            if (!bookingOk)
            {
                await _repo.UpdatePaymentStatusAsync(req.MockOrderId, "Failed");
                _logger.LogError("ProcessPayment: booking update to Booked failed after successful payment for order {OrderId}; payment marked Failed.", req.MockOrderId);
                return (false, "Booking update failed");
            }

            _logger.LogInformation("ProcessPayment succeeded for order {OrderId}, booking {BookingId}.", req.MockOrderId, bookingId);
            return (true, "Payment completed successfully");
        }

        public async Task<PaymentStatusResponse?> GetStatusAsync(int bookingId)
        {
            var payment = await _repo.GetPaymentByBookingIdAsync(bookingId);
            var booking = await _repo.GetBookingForPaymentAsync(bookingId);
            if (booking == null) return null;

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