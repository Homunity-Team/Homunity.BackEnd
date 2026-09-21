using Homunity_Shared_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public interface IChatService
    {
        Task<ChatResponse> SendMessageAsync(int studentId, string userMessage);
        Task<List<ChatMessageResponse>> GetHistoryAsync(int studentId);   // كان: List<ChatMessageDTO>
        Task<bool> ClearHistoryAsync(int studentId);

    }
}
