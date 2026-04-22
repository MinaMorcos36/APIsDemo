namespace APIsDemo.Models
{
    public class CareerChatRequest
    {
        public int ConversationId { get; set; }
        public int? CvId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
