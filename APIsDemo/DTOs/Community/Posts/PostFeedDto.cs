namespace APIsDemo.DTOs.Community.Posts
{
    public class PostFeedDto
    {
        public int Id { get; set; }

        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public int AuthorId { get; set; }
        public string AuthorType { get; set; } = null!;
        public string AuthorName { get; set; } = null!;

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

        public bool IsLikedByMe { get; set; }
        public bool IsSavedByMe { get; set; }
    }
}
