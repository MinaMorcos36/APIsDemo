using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Community.Comments
{
    public class CreateCommentDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        [RegularExpression(@".*\S.*")]
        public string Content { get; set; } = null!;

        // Optional ? null = comment on post
        [Range(1, int.MaxValue)]
        public int? ParentCommentId { get; set; }
    }
}
