namespace ProGrow.API.DTOs.Community.Jobs
{
    public class ApplyJobDto
    {
        public string? CoverLetter { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PortfolioLink { get; set; }
    }
}
