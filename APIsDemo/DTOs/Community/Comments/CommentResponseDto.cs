namespace APIsDemo.DTOs.Community.Comments
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string AuthorType { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
