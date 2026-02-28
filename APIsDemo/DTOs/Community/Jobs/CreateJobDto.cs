using System.ComponentModel.DataAnnotations;

namespace APIsDemo.DTOs.Community.Jobs
{
    public class CreateJobDto
    {
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
