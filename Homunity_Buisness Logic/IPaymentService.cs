using Homunity_Shared_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public interface IPaymentService
    {
        Task<PaymentOrderResponse?> CreateOrderAsync(int bookingId, int studentId);
        Task<(bool success, string message)> ProcessPaymentAsync(MockProcessRequest request);
        Task<PaymentStatusResponse?> GetStatusAsync(int bookingId);

    }


}