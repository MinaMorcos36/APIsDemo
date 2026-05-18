using ProGrow.API.DTOs.Community.Comments;
using ProGrow.API.Services.Interfaces.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProGrow.API.Controllers.Community
{
    [Route("api/posts/{postId}/comments")]
    [ApiController]
    [Authorize]
    public class PostCommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public PostCommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePostComment(int postId, [FromBody] CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Comment cannot be empty");

            var result = await _commentService.CreateAsync(postId, dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPostComments(int postId)
        {
            var comments = await _commentService.GetByPostIdAsync(postId);
            return Ok(comments);
        }
    }
}
