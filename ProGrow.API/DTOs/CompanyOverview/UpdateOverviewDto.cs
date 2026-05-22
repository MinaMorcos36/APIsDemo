using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.CompanyOverview
{
    public class UpdateOverviewDto
    {
        [Range(1, int.MaxValue)]
        public int? IndustryId { get; set; }
        [MaxLength(50)]
        public string? Name { get; set; }
        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        [MaxLength(30)]
        [Phone]
        public string? Phone { get; set; }
        [MaxLength(255)]
        public string? Address { get; set; }
        [MaxLength(2000)]
        public string? Overview { get; set; }
        [Url]
        public string? WebsiteUrl { get; set; }
        [Url]
        public string? PictureUrl { get; set; }

    }
}
