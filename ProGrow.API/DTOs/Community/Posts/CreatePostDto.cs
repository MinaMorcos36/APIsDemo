using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Community.Posts
{
    public class CreatePostDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        [RegularExpression(@".*\S.*")]
        public string Content { get; set; } = null!;
    }
}
