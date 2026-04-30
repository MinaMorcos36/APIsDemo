using APIsDemo.Models;

public class ConversationModel
{
    public int Id { get; set; }

    public int UserId { get; set; }   // FK only

    public User User { get; set; }    // navigation

    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ChatMessageModel> Messages { get; set; } = new();
}