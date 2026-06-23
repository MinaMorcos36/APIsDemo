using ProGrow.API.DTOs.Community.Jobs;
using ProGrow.API.DTOs.Community.Posts;

namespace ProGrow.API.DTOs.Community.Feed
{
    public class FeedItemDto
    {
        public string Type { get; set; } = null!;
        // "Post" or "Job"

        public DateTime CreatedAt { get; set; }

        public PostFeedDto? Post { get; set; }

        public JobFeedDto? Job { get; set; }

        // Convenience flag for frontend: whether the current viewer follows the item author
        public bool IsFollowedByMe { get; set; }
    }
}
