namespace APIsDemo.DTOs.Community.Jobs
{
    public class JobResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; } = null;
        public string? Location { get; set; } = null;
        public DateTime CreatedAt { get; set; }
        public int? AuthorId { get; set; }
    }
}
