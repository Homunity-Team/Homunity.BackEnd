using Homunity_Data_Access.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public interface IBookingRepository
    {
        Task<int> AddAsync(int propertyId, int studentId, int statusId);
        Task<bool> UpdateStatusAsync(int bookingId, int statusId, DateTime? confirmedAt);
        Task<bool> ConfirmWithTransactionAsync(int bookingId, int propertyId, DateTime confirmedAt);
        Task<BookingDetails?> FindAsync(int bookingId);
        Task<List<BookingListItem>> GetByStudentIdAsync(int studentId);
        Task<List<BookingListItem>> GetByOwnerIdAsync(int ownerId);
        Task<List<BookingByPropertyItem>> GetByPropertyIdAsync(int propertyId);
        Task<bool> IsPropertyAlreadyBookedAsync(int propertyId);
        Task<bool> IsStudentAlreadyRequestedPropertyAsync(int studentId, int propertyId);
        Task<bool> IsPropertyExistAsync(int propertyId);
        Task<bool> IsUserInRoleAsync(int userId, string roleName);
        Task<bool> IsBookingStatusValidAsync(int statusId);
    }
}
