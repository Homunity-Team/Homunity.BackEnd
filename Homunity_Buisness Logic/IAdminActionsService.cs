using Homunity_Data_Access.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public interface IAdminActionsService
    {
        Task<DashboardStatsResult> GetDashboardStatsAsync();
        Task<List<AdminPropertyProjection>> GetRecentActionsAsync(int pageSize);
        Task<(List<AdminPropertyProjection> Items, int TotalCount)> GetPendingPropertiesAsync(int page, int pageSize);
        Task<List<AdminPropertyProjection>> GetRejectedPropertiesAsync();
        Task<AdminPropertyDetail?> GetPropertyDetailAsync(int propertyId);
        Task<bool> ApproveAsync(int propertyId, int adminId);
        Task<bool> RejectAsync(int propertyId, int adminId, string reason);
    }

}
