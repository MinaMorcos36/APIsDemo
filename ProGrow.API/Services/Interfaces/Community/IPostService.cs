using ProGrow.API.DTOs.Community.Posts;
using Microsoft.AspNetCore.Http;

namespace ProGrow.API.Services.Interfaces.Community
{
    public interface IPostService
    {
        Task<PostResponseDto> CreateAsync(CreatePostDto dto, IFormFile? mediaFile);
        Task<List<PostFeedDto>> GetFeedAsync(int? page = null, int? pageSize = null);
    }
}
