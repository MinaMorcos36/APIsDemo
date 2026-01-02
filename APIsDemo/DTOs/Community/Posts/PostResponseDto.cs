namespace APIsDemo.DTOs.Community.Posts
{
    public class PostResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Ownership
        public int? AuthorId { get; set; }
        public string? AuthorType { get; set; }

        // Engagement
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

        // Current user state
        public bool IsLikedByMe { get; set; }
        public bool IsSavedByMe { get; set; }
    }
}
