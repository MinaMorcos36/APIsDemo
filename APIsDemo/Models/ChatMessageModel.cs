namespace APIsDemo.Models
{
    public class ChatMessageModel
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public ConversationModel Conversation { get; set; } = null!;

        public string Role { get; set; } = string.Empty; // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }
}
