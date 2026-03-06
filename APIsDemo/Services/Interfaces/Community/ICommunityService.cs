using APIsDemo.DTOs.Community.Feed;

namespace APIsDemo.Services.Interfaces.Community
{
    public interface ICommunityService
    {
        Task<List<FeedItemDto>> GetFeedAsync();
    }
}
