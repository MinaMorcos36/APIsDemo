namespace APIsDemo.DTOs.Community.Jobs
{
    public class CompanysJobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }

        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;

        public int ApplicantsCount { get; set; }
        public bool IsActive { get; set; }
    }
}
