using ProGrow.API.DTOs.Community.Feed;

namespace ProGrow.API.Services.Interfaces.Community
{
    public interface ICommunityService
    {
        Task<List<FeedItemDto>> GetFeedAsync();
    }
}
