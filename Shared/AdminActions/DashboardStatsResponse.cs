using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.AdminActions
{
    public class DashboardStatsResponse
    {
        public int TotalProperties { get; set; }
        public int PendingProperties { get; set; }
        public int ApprovedProperties { get; set; }
        public int RejectedProperties { get; set; }
        public int TotalBookings { get; set; }
        public int TotalUsers { get; set; }
    }

}
