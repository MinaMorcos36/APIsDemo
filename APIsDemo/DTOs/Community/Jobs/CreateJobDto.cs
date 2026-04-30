namespace APIsDemo.DTOs.Community.Jobs
{
    public class CreateJobDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
    }
}