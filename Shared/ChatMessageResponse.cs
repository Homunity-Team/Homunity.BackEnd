namespace Homunity_Shared_DTOs
{
    public class ChatMessageResponse
    {
        public int MessageId { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
