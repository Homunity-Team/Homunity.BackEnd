using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class ReferenceDataService : IReferenceDataService
    {
        private readonly IRoleRepository _roles;
        private readonly IServiceRepository _services;
        private readonly IBookingStatusRepository _bookingStatuses;

        public ReferenceDataService(
            IRoleRepository roles,
            IServiceRepository services,
            IBookingStatusRepository bookingStatuses)
        {
            _roles = roles;
            _services = services;
            _bookingStatuses = bookingStatuses;
        }

        public Task<List<RoleEntity>> GetRolesAsync() => _roles.GetAllAsync();
        public Task<List<ServiceEntity>> GetServicesAsync() => _services.GetAllAsync();
        public Task<List<BookingStatusEntity>> GetBookingStatusesAsync() => _bookingStatuses.GetAllAsync();
        public Task<BookingStatusEntity?> GetBookingStatusByIdAsync(int id) => _bookingStatuses.FindAsync(id);
    }
}
