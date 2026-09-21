using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    // التجريد الذي يعزل كل تفاصيل مزوّد Gemini عن ChatService. أي استبدال لمزوّد الذكاء
    // الاصطناعي مستقبلًا يتطلب تنفيذ هذا الـinterface فقط، بدون لمس منطق المحادثة التجاري.
    public interface IGeminiClient
    {
        Task<GeminiReplyResult> GenerateReplyAsync(
            string systemPrompt,
            List<GeminiHistoryMessage> history,
            CancellationToken cancellationToken = default);
    }

}
