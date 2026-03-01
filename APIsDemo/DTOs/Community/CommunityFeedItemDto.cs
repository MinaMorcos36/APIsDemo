using System;

namespace APIsDemo.DTOs.Community
{
    public class CommunityFeedItemDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!; // "Post" or "Job"

        // For jobs: Title populated. For posts: null.
        public string? Title { get; set; }

        // Content for both posts and jobs (post content or job description)
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public int AuthorId { get; set; }
        public string AuthorType { get; set; } = null!;
        public string AuthorName { get; set; } = null!;

        // Post-specific
        public int? LikesCount { get; set; }
        public int? CommentsCount { get; set; }
        public bool? IsLikedByMe { get; set; }
        public bool? IsSavedByMe { get; set; }

        // Job-specific
        public string? Location { get; set; }
    }
}
