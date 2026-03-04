using APIsDemo.DTOs.Community.Comments;

namespace APIsDemo.Services.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponseDto> CreateAsync(int postId, CreateCommentDto dto);
        Task<IEnumerable<CommentDto>> GetByPostIdAsync(int postId);
    }
}
