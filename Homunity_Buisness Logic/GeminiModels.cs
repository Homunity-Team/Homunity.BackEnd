using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    // شكل رسالة تاريخ محادثة، مستقل عن ChatMessageEntity (طبقة البيانات) —
    // GeminiClient لا يعرف شيئًا عن EF Core أو قاعدة البيانات.
    public class GeminiHistoryMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class GeminiReplyResult
    {
        public bool Success { get; set; }
        public string? ReplyText { get; set; }
        public string? FailureReason { get; set; }

        public static GeminiReplyResult Ok(string text) => new() { Success = true, ReplyText = text };
        public static GeminiReplyResult Fail(string reason) => new() { Success = false, FailureReason = reason };
    }

}
