namespace APIsDemo.DTOs.Community.Jobs
{
    public class JobFeedDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; } = null;
        public string? Location { get; set; } = null;
        public DateTime CreatedAt { get; set; }
        public int AuthorId { get; set; }
        public string? AuthorName { get; set;} = null!;
    }
}
