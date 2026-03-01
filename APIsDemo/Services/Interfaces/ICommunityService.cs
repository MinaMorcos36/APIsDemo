using APIsDemo.DTOs.Community;

namespace APIsDemo.Services.Interfaces
{
    public interface ICommunityService
    {
        Task<List<CommunityFeedItemDto>> GetFeedAsync(int authorId, string authorType, int page = 1, int pageSize = 20);
    }
}
