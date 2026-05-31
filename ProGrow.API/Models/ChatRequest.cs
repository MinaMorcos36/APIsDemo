namespace ProGrow.API.Models
{
    public class ChatRequest
    {
        public int? ConversationId { get; set; }

        public string Message { get; set; } = string.Empty;

        public IFormFile? File { get; set; }
    }
}
