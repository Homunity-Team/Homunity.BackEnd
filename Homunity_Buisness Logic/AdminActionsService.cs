using Homunity_Data_Access.Repositories;
using Homunity_Data_Access.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class AdminActionsService : IAdminActionsService
    {
        private const int STATUS_APPROVED = 2, STATUS_REJECTED = 3;
        private readonly IAdminActionsRepository _repo;
        public AdminActionsService(IAdminActionsRepository repo) => _repo = repo;

        public Task<DashboardStatsResult> GetDashboardStatsAsync() => _repo.GetDashboardStatsAsync();
        public Task<List<AdminPropertyProjection>> GetRecentActionsAsync(int pageSize) => _repo.GetRecentActionsAsync(pageSize);

        public Task<(List<AdminPropertyProjection>, int)> GetPendingPropertiesAsync(int page, int pageSize) =>
            _repo.GetPendingPropertiesAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 10 : pageSize);

        public Task<List<AdminPropertyProjection>> GetRejectedPropertiesAsync() => _repo.GetRejectedPropertiesAsync();
        public Task<AdminPropertyDetail?> GetPropertyDetailAsync(int propertyId) => _repo.GetPropertyDetailAsync(propertyId);

        public async Task<bool> ApproveAsync(int propertyId, int adminId)
        {
            if (adminId <= 0 || !await _repo.IsAdminValidAsync(adminId)) return false;

            var property = await _repo.GetPropertyDetailAsync(propertyId);
            if (property == null) return false;

            // In ApproveAsync:
            if (!PropertyStatusPolicy.CanChangeStatus(property.StatusId, STATUS_APPROVED)) return false;

            return await _repo.UpdatePropertyStatusAsync(propertyId, STATUS_APPROVED);
        }

        public async Task<bool> RejectAsync(int propertyId, int adminId, string reason)
        {
            if (adminId <= 0 || !await _repo.IsAdminValidAsync(adminId)) return false;
            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 10) return false;

            var property = await _repo.GetPropertyDetailAsync(propertyId);
            if (property == null) return false;
           
            // In RejectAsync:
            if (!PropertyStatusPolicy.CanChangeStatus(property.StatusId, STATUS_REJECTED)) return false;
            if (!await _repo.SaveRejectReasonAsync(propertyId, reason.Trim())) return false;

            return await _repo.UpdatePropertyStatusAsync(propertyId, STATUS_REJECTED);
        }
    }

}
