using APIsDemo.DTOs.Community.Feed;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces.Community;

namespace APIsDemo.Services.Implementations.Community
{
    public class CommunityService : ICommunityService
    {
        private readonly AppDbContext _context;
        private readonly IPostService _postService;
        private readonly IJobService _jobService;

        public CommunityService(
            AppDbContext context,
            IPostService postService,
            IJobService jobService)
        {
            _context = context;
            _postService = postService;
            _jobService = jobService;
        }

        public async Task<List<FeedItemDto>> GetFeedAsync()
        {
            var posts = await _postService.GetFeedAsync();
            var jobs = await _jobService.GetFeedAsync();

            var feed = new List<FeedItemDto>();

            feed.AddRange(posts.Select(p => new FeedItemDto
            {
                Type = "Post",
                CreatedAt = p.CreatedAt,
                Post = p
            }));

            feed.AddRange(jobs.Select(j => new FeedItemDto
            {
                Type = "Job",
                CreatedAt = j.CreatedAt,
                Job = j
            }));

            return feed
                .OrderByDescending(f => f.CreatedAt)
                .ToList();
        }
    }
}
