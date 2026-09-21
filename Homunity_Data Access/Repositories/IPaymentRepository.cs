using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public interface IPaymentRepository
    {
        Task<BookingForPayment?> GetBookingForPaymentAsync(int bookingId);
        Task<BookingLockInfo?> GetBookingForPaymentWithLockAsync(int bookingId);
        Task<bool> HasPendingPaymentAsync(int bookingId);
        Task<int> CreatePaymentAsync(int bookingId, int studentId, int ownerId, int propertyId, decimal amount, string mockOrderId);
        Task<bool> UpdatePaymentStatusAsync(string mockOrderId, string status);
        Task<PaymentEntity?> GetPaymentByBookingIdAsync(int bookingId);
        Task<PaymentEntity?> GetPaymentByMockOrderIdAsync(string mockOrderId);
        Task<bool> UpdateBookingStatusToBookedAsync(int bookingId);
    }

}
