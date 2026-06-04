using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Community.Jobs
{
    public class ApplyJobDto
    {
        [StringLength(4000)]
        public string? CoverLetter { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Url]
        public string? PortfolioLink { get; set; }

        public int? CvId { get; set; }
    }
}
