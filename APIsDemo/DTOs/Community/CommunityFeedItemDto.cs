using System;

namespace APIsDemo.DTOs.Community
{
    public class CommunityFeedItemDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!; // "Post" or "Job"

        public string? Title { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public int AuthorId { get; set; }
        public string AuthorType { get; set; } = null!;
        public string AuthorName { get; set; } = string.Empty;

        public int? LikesCount { get; set; }
        public int? CommentsCount { get; set; }
        public bool? IsLikedByMe { get; set; }
        public bool? IsSavedByMe { get; set; }

        public string? Location { get; set; }
    }
}
