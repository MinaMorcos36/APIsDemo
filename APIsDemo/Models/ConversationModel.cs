namespace APIsDemo.Models
{
    public class ConversationModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; } // هنربطه بـ Identity بعدين

        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ChatMessageModel> Messages { get; set; } = new();
    }
}
