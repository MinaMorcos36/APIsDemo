using System.Collections.Generic;

namespace ProGrow.API.DTOs.Community.Feed
{
    public class PagedFeedResponse
    {
        public List<FeedItemDto> Items { get; set; } = new List<FeedItemDto>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
