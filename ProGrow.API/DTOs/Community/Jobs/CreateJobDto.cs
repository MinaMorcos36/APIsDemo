using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Community.Jobs
{
    public class CreateJobDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        [RegularExpression(@".*\S.*")]
        public string Title { get; set; } = null!;

        [StringLength(4000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }
    }
}
