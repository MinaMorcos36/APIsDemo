using APIsDemo.DTOs.Community.Comments;

namespace APIsDemo.Services.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponseDto> CreateAsync(int postId, int userId, string authorType, CreateCommentDto dto);
    }
}
