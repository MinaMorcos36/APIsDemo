using APIsDemo.DTOs.Community.Comments;

namespace APIsDemo.Services.Interfaces.Community
{
    public interface ICommentService
    {
        Task<CommentResponseDto> CreateAsync(int postId, CreateCommentDto dto);
        Task<IEnumerable<CommentDto>> GetByPostIdAsync(int postId);
    }
}
