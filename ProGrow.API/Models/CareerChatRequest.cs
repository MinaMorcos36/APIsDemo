using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.Models
{
    public class CareerChatRequest
    {
        [Range(1, int.MaxValue)]
        public int? ConversationId { get; set; }

        [Range(1, int.MaxValue)]
        public int? CvId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        [RegularExpression(@".*\S.*")]
        public string Message { get; set; } = string.Empty;
    }
}
