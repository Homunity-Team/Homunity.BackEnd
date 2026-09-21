using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.AdminActions
{
    // تمثل نتيجة فعل إداري وحيد (Approve أو Reject) على عقار — راجع تحليل Sprint 3 (Question 4)
    public class AdminActionResponse
    {
        public int PropertyId { get; set; }
        public int AdminId { get; set; }
        // "Approved" أو "Rejected"
        public string Action { get; set; }
        public string? RejectReason { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }
}
