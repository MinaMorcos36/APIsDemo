using System.ComponentModel.DataAnnotations;

namespace APIsDemo.DTOs.CompanyOverview
{
    public class UpdateOverviewDto
    {
        public int? IndustryId { get; set; }
        [MaxLength(50)]
        public string? Name { get; set; }
        [MaxLength(30)]
        public string? Phone { get; set; }
        [MaxLength(255)]
        public string? Address { get; set; }
        public string? Overview { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? PictureUrl { get; set; }

    }
}
