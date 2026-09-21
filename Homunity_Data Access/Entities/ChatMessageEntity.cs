using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Entities
{
    public class ChatMessageEntity
    {
        public int MessageId { get; set; }
        public int StudentId { get; set; }
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
    }
}
