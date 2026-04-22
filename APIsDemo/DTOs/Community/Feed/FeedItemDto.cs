using APIsDemo.DTOs.Community.Jobs;
using APIsDemo.DTOs.Community.Posts;
using APIsDemo.DTOs.Community.Jobs;

namespace APIsDemo.DTOs.Community.Feed
{
    public class FeedItemDto
    {
        public string Type { get; set; } = null!;
        // "Post" or "Job"

        public DateTime CreatedAt { get; set; }

        public PostFeedDto? Post { get; set; }

        public JobFeedDto? Job { get; set; }
    }
}
