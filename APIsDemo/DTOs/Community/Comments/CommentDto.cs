namespace APIsDemo.DTOs.Community.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public int AuthorId { get; set; }
        public string? AuthorType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<CommentDto>? Replies { get; set; }
    }
}
