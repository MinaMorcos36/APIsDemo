namespace ProGrow.API.DTOs.CompanyOverview
{
    public class OverviewResponseDto
    {
        public int? IndustryId { get; set; }
        public string IndustryName { get; set; } = null!;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Overview { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? PictureUrl { get; set; }
        public bool IsFollowedByMe { get; set; }
    }
}
