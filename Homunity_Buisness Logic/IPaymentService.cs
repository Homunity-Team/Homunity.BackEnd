using Homunity_Shared_DTOs;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public interface IPaymentService
    {
        Task<PaymentOrderResponse?> CreateOrderAsync(int bookingId, int studentId);
        Task<(bool success, string message)> ProcessPaymentAsync(MockProcessRequest request, int actingStudentId);
        Task<PaymentStatusResponse?> GetStatusAsync(int bookingId, int actingUserId, bool isOwnerOrAdmin);
    }
}
