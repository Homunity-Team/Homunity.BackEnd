using Homunity_Data_Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class clsPropertyStatus
    {
        // =============================================
        // Property Status IDs Constants
        // =============================================
        private const int STATUS_PENDING = 1;
        private const int STATUS_APPROVED = 2;
        private const int STATUS_REJECTED = 3;

        // =============================================
        // CAN CHANGE STATUS — State Machine
        // =============================================
        public static bool CanChangeStatus(int oldStatusId, int newStatusId)
        {
            // Pending → Approved ✅
            // Pending → Rejected ✅
            // Approved → أي حاجة ❌
            // Rejected → أي حاجة ❌

            if (oldStatusId == STATUS_PENDING &&
               (newStatusId == STATUS_APPROVED || newStatusId == STATUS_REJECTED))
                return true;

            return false;
        }




        public static bool IsValidStatus(int statusId)
        {
            if (statusId <= 0)
                return false;

            return clsPropertyStatusData.IsStatusExists(statusId);
        }
    }
}
