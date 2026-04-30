
namespace APIsDemo.DTOs.Community.Jobs
{
    public class JobApplicationDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int ApplicantId { get; set; }
        public string ApplicantEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}