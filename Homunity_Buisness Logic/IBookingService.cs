using Homunity_Data_Access.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public interface IBookingService
    {
        Task<BookingCreateResult> CreateAsync(int propertyId, int studentId);
        Task<BookingDetails?> GetByIdAsync(int bookingId);
        Task<List<BookingListItem>> GetByStudentIdAsync(int studentId);
        Task<List<BookingListItem>> GetByOwnerIdAsync(int ownerId);
        Task<List<BookingByPropertyItem>> GetByPropertyIdAsync(int propertyId);
        Task<BookingActionResult> ConfirmAsync(int bookingId, int ownerId);
        Task<BookingActionResult> CancelAsync(int bookingId);
    }

}
