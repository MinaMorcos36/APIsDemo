using APIsDemo.DTOs.Community.Posts;

namespace APIsDemo.Services.Interfaces
{
    public interface IPostService
    {
        Task<PostResponseDto> CreateAsync(CreatePostDto dto);
        Task<List<PostFeedDto>> GetFeedAsync();
    }
}
