namespace ProGrow.API.DTOs.Community.Jobs
{
    public class JobDetailsDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyPictureUrl { get; set; }
        public string? Address { get; set; }
        public string CityOffice { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string LocationMode { get; set; } = string.Empty;
        public string? JobType { get; set; }
        public int JobCategoryId { get; set; }
        public string JobCategoryName { get; set; } = string.Empty;
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public bool IsSalaryInInterview { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public string AboutRole { get; set; } = string.Empty;
        public string Responsibilities { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public required string BannerImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int TotalApplications { get; set; }
        public DateTime createdAt { get; set; }
    }
}
