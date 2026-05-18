using ProGrow.API.DTOs.Community.Posts;

namespace ProGrow.API.Services.Interfaces.Community
{
    public interface IPostService
    {
        Task<PostResponseDto> CreateAsync(CreatePostDto dto);
        Task<List<PostFeedDto>> GetFeedAsync();
    }
}
