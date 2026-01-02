namespace APIsDemo.DTOs.Community.Comments
{
    public class CreateCommentDto
    {
        public string Content { get; set; } = null!;

        // Optional → null = comment on post
        public int? ParentCommentId { get; set; }
    }
}
