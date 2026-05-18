using ProGrow.API.DTOs.Community.Comments;

namespace ProGrow.API.Services.Interfaces.Community
{
    public interface ICommentService
    {
        Task<CommentResponseDto> CreateAsync(int postId, CreateCommentDto dto);
        Task<IEnumerable<CommentDto>> GetByPostIdAsync(int postId);
        Task<CommentResponseDto> CreateForJobAsync(int jobId, CreateCommentDto dto);
        Task<IEnumerable<CommentDto>> GetByJobIdAsync(int jobId);
    }
}
