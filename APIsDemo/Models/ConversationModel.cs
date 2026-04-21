namespace APIsDemo.Models
{
    public class ConversationModel
    {
        public int Id { get; set; }
        public User User { get; set; }
        public int? UserId { get; set; } // هنربطه بـ Identity بعدين

        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ChatMessageModel> Messages { get; set; } = new();

    }
}
