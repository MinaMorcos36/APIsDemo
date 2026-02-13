using APIsDemo.DTOs.Community.Comments;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIsDemo.Controllers.Community
{
    [Route("api/posts/{postId}/comments")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        private (int AuthorId, string AuthorType) GetAuthor()
        {
            return (
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                User.FindFirstValue("AuthorType")!
            );
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment( int postId, [FromBody] CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Comment cannot be empty");

            var (authorId, authorType) = GetAuthor();

            var result = await _commentService.CreateAsync(postId, authorId, authorType, dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _commentService.GetByPostIdAsync(postId);
            return Ok(comments);
        }
    }
}
