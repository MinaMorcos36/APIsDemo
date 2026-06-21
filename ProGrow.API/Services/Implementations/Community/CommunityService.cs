using System.Linq;
using ProGrow.API.DTOs.Community.Feed;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Community;

namespace ProGrow.API.Services.Implementations.Community
{
    public class CommunityService : ICommunityService
    {
        private readonly IPostService _postService;
        private readonly IJobService _jobService;

        public CommunityService(
            IPostService postService,
            IJobService jobService)
        {
            _postService = postService;
            _jobService = jobService;
        }

        public async Task<PagedFeedResponse> GetFeedAsync(int? page = null, int? pageSize = null)
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

            var ordered = feed.OrderByDescending(f => f.CreatedAt).ToList();

            // Calculate totals before paging
            var totalCount = ordered.Count;

            if (pageSize == null || pageSize <= 0)
            {
                return new PagedFeedResponse
                {
                    Items = ordered,
                    TotalCount = totalCount,
                    TotalPages = 1
                };
            }

            var size = Math.Min(pageSize.Value, 100);
            var currentPage = Math.Max(1, page.GetValueOrDefault(1));

            var totalPages = (int)Math.Ceiling((double)totalCount / size);

            var items = ordered
                .Skip((currentPage - 1) * size)
                .Take(size)
                .ToList();

            return new PagedFeedResponse
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = Math.Max(1, totalPages)
            };
        }
    }
}
