using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    // Extracted from clsPropertyStatus.cs, logic unchanged. Pure in-memory state-machine rule,
    // no data access — so it does not need a repository. Kept as a small standalone policy
    // (not inside PropertyService) because its only live caller is AdminActionsService, not PropertyService.
    public static class PropertyStatusPolicy
    {
        private const int STATUS_PENDING = 1;
        private const int STATUS_APPROVED = 2;
        private const int STATUS_REJECTED = 3;

        public static bool CanChangeStatus(int oldStatusId, int newStatusId)
        {
            // Pending → Approved 
            // Pending → Rejected 
            // Approved → anything 
            // Rejected → anything 
            return oldStatusId == STATUS_PENDING &&
                   (newStatusId == STATUS_APPROVED || newStatusId == STATUS_REJECTED);
        }
    }
}
