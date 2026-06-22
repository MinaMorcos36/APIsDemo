namespace ProGrow.API.DTOs.Admin.Dashboard
{
    public class RecentJobDto
    {
        public int JobId { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
