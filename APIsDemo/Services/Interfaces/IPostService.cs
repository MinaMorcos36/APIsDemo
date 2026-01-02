using APIsDemo.DTOs.Community.Posts;

namespace APIsDemo.Services.Interfaces
{
    public interface IPostService
    {
        Task<PostResponseDto> CreateAsync(CreatePostDto dto, int userId, string authorType);
        Task<List<PostFeedDto>> GetFeedAsync(int authorId, string authorType);
    }
}
