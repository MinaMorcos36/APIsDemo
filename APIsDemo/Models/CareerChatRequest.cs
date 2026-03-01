namespace APIsDemo.Models
{
    public class CareerChatRequest
    {
        public Guid ConversationId { get; set; }
        public Guid? CvId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
