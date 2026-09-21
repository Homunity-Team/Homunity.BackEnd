using Homunity_Data_Access.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    /// <summary>
    /// Thin BLL facade for read-only reference data so controllers do not inject repositories.
    /// </summary>
    public interface IReferenceDataService
    {
        Task<List<RoleEntity>> GetRolesAsync();
        Task<List<ServiceEntity>> GetServicesAsync();
        Task<List<BookingStatusEntity>> GetBookingStatusesAsync();
        Task<BookingStatusEntity?> GetBookingStatusByIdAsync(int id);
    }
}
