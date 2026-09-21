using Homunity_Data_Access.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public interface IChatRepository
    {
        Task<List<ChatMessageEntity>> GetHistoryAsync(int studentId, int limit);
        Task SaveMessageAsync(int studentId, string role, string content);
        Task<bool> ClearHistoryAsync(int studentId);
    }

}
