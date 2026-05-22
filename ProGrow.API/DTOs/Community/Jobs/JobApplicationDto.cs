
namespace ProGrow.API.DTOs.Community.Jobs
{
    public class JobApplicationDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string? JobDescription { get; set; }
        public string? JobLocation { get; set; }
        public string? JobShortDescription { get; set; }
        public string? JobLocationMode { get; set; }
        public string? JobType { get; set; }
        public string? JobCityOffice { get; set; }
        public decimal? JobSalaryFrom { get; set; }
        public decimal? JobSalaryTo { get; set; }
        public bool JobIsSalaryInInterview { get; set; }
        public string? JobBannerImageUrl { get; set; }
        public string? JobAboutRole { get; set; }
        public string? JobResponsibilities { get; set; }
        public string? JobRequirements { get; set; }
        public DateTime? JobUpdatedAt { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int ApplicantId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantHeadline { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public int? CvId { get; set; }
        public string CvFileName { get; set; } = string.Empty;
        public string? CvText { get; set; }
        public string? CvLanguage { get; set; }
        public int? CvScore { get; set; }
        public string? CvScoreReason { get; set; }
        public string? CoverLetter { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PortfolioLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}
