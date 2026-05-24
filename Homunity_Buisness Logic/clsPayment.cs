using Homunity_Data_Access;
using Homunity_Shared_DTOs;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class clsPayment
    {
        // Mock test cards
        private static readonly string[] ValidCards = {
        "4111111111111111",  // Visa test
        "5500005555555559",  // Mastercard test
        "4000000000000002"  }; // Visa test 2

        // Step 1: Owner confirms → status becomes "Confirmed"
        public static bool ConfirmBooking(int bookingId, int ownerId)
        {
            var booking = clsPaymentData.GetBookingForPayment(bookingId);
            if (booking == null) return false;
            // Verify owner owns this property
            if (Convert.ToInt32(booking["OwnerID"]) != ownerId) return false;
            return clsPaymentData.UpdateBookingStatusByName(bookingId, "Confirmed");
        }





        // Step 2: Student initiates payment → create mock order
        // Step 2: Student initiates payment → create mock order
        public static PaymentOrderResponse CreateOrder(int bookingId, int studentId)
        {
            var booking = clsPaymentData.GetBookingForPayment(bookingId);
            if (booking == null) return null;

            if (!booking["StatusName"].ToString().Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
                return null;

            if (clsPaymentData.HasPendingPayment(bookingId))
                return null;

            decimal amount = Convert.ToDecimal(booking["Price"]) * 2;

            string mockOrderId = $"HMNT-{bookingId}-{DateTime.Now.Ticks}";

            clsPaymentData.CreatePayment(
                bookingId,
                studentId,
                Convert.ToInt32(booking["OwnerID"]),
                Convert.ToInt32(booking["PropertyId"]),
                amount,
                mockOrderId
            );

            return new PaymentOrderResponse
            {
                MockOrderId = mockOrderId,
                BookingId = bookingId,
                Amount = amount,
                Status = "created"
            };
        }



        // Step 3: Student submits mock card → process payment
        public static (bool success, string message) ProcessPayment(MockProcessRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.MockOrderId))
                return (false, "Invalid request");

            string cleanCard = req.CardNumber?.Replace(" ", "") ?? "";

            bool cardValid = ValidCards.Contains(cleanCard);
            if (!cardValid)
                return (false, "Invalid card");

            var parts = req.MockOrderId.Split('-');
            if (parts.Length < 2 || !int.TryParse(parts[1], out int bookingId))
                return (false, "Invalid order");

            var booking = clsPaymentData.GetBookingForPaymentWithLock(bookingId);

            if (booking == null)
                return (false, "Booking not found");

            if (!booking["StatusName"].ToString().Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
                return (false, "Booking not confirmed");

            // 1️⃣ Update Payment
            bool paymentOk = clsPaymentData.UpdatePaymentStatus(req.MockOrderId, "Success");
            if (!paymentOk)
                return (false, "Payment update failed");

            // 2️⃣ Delegate booking update to Booking layer (IMPORTANT FIX)
            bool bookingOk = clsBookingData.UpdateBookingStatusToBooked(bookingId);

            if (!bookingOk)
            {
                // لو الـ booking update فشل، ارجع الـ payment لـ Failed
                clsPaymentData.UpdatePaymentStatus(req.MockOrderId, "Failed");
                return (false, "Booking update failed");
            }
            return (true, "Payment completed successfully");
        }
        // Step 4: Get payment status
        public static PaymentStatusResponse GetStatus(int bookingId)
        {
            var payment = clsPaymentData.GetPaymentByBookingId(bookingId);
            var booking = clsPaymentData.GetBookingForPayment(bookingId);
            if (booking == null) return null;

            return new PaymentStatusResponse
            {
                BookingId = bookingId,
                PaymentStatus = payment != null ? payment["Status"].ToString() : "NotCreated",
                BookingStatus = booking["StatusName"].ToString(),
                Amount = payment != null ? Convert.ToDecimal(payment["Amount"]) : 0,
                PaidAt = payment != null && payment["PaidAt"] != DBNull.Value
                                ? Convert.ToDateTime(payment["PaidAt"]).ToString("yyyy-MM-dd HH:mm") : null
            };
        }
    }
}
