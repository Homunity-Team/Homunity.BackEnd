using Homunity_Data_Access.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public interface IAdminActionsRepository
    {
        Task<DashboardStatsResult> GetDashboardStatsAsync();
        Task<List<AdminPropertyProjection>> GetRecentActionsAsync(int pageSize);
        Task<bool> IsAdminValidAsync(int adminId);
        Task<(List<AdminPropertyProjection> Items, int TotalCount)> GetPendingPropertiesAsync(int page, int pageSize);
        Task<List<AdminPropertyProjection>> GetRejectedPropertiesAsync();
        Task<AdminPropertyDetail?> GetPropertyDetailAsync(int propertyId);
        Task<bool> UpdatePropertyStatusAsync(int propertyId, int statusId);
        Task<bool> SaveRejectReasonAsync(int propertyId, string reason);
    }

}
