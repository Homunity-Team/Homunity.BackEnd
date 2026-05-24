using Homunity_Data_Access;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class clsAdminActions
    {
        // Status Constants
        private const int STATUS_PENDING = 1;
        private const int STATUS_APPROVED = 2;
        private const int STATUS_REJECTED = 3;



        // =============================================
        // GET DASHBOARD STATS ✅
        // =============================================
        public static object GetDashboardStats()
        {
            int totalProperties = 0;
            int pendingProperties = 0;
            int approvedProperties = 0;
            int rejectedProperties = 0;
            int totalBookings = 0;
            int totalUsers = 0;

            bool found = clsAdminActionsData.GetDashboardStats(
                ref totalProperties,
                ref pendingProperties,
                ref approvedProperties,
                ref rejectedProperties,
                ref totalBookings,
                ref totalUsers);

            if (!found)
                return null;

            return new
            {
                totalProperties = totalProperties,
                pendingProperties = pendingProperties,
                approvedProperties = approvedProperties,
                rejectedProperties = rejectedProperties,
                totalBookings = totalBookings,
                totalUsers = totalUsers
            };
        }


        public static List<clsProperties> GetRecentActions(int pageSize = 10)
        {
            var dt = clsAdminActionsData.GetRecentActions(pageSize);
            var list = new List<clsProperties>();

            foreach (DataRow row in dt.Rows)
            {
                var property = new clsProperties();
                property.LoadFromDataRow(row);
                list.Add(property);
            }

            return list;
        }

     }
}
