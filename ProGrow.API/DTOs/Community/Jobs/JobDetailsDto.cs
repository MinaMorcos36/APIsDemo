namespace ProGrow.API.DTOs.Community.Jobs
{
    public class JobDetailsDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyPictureUrl { get; set; }
        public string CityOffice { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string LocationMode { get; set; } = string.Empty;
        public string? JobType { get; set; }
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public bool IsSalaryInInterview { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public string AboutRole { get; set; } = string.Empty;
        public string Responsibilities { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
    }
}
