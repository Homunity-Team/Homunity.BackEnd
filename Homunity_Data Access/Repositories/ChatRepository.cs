using Homunity_Data_Access.Data;
using Homunity_Data_Access.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly HomunityDbContext _db;
        public ChatRepository(HomunityDbContext db) => _db = db;

        public async Task<List<ChatMessageEntity>> GetHistoryAsync(int studentId, int limit)
        {
            var messages = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.StudentId == studentId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .ToListAsync();

            messages.Reverse();
            return messages;
        }

        public async Task SaveMessageAsync(int studentId, string role, string content)
        {
            _db.ChatMessages.Add(new ChatMessageEntity { StudentId = studentId, Role = role, Content = content, CreatedAt = DateTime.Now });
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ClearHistoryAsync(int studentId)
        {
            var messages = await _db.ChatMessages.Where(m => m.StudentId == studentId).ToListAsync();
            if (messages.Count == 0) return true;
            _db.ChatMessages.RemoveRange(messages);
            await _db.SaveChangesAsync();
            return true;
        }
    }

}
