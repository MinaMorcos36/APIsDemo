using ProGrow.API.DTOs.Community.Comments;
using ProGrow.API.Services.Interfaces.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProGrow.API.Controllers.Community
{
    [Route("api/jobs/{jobId}/comments")]
    [ApiController]
    [Authorize]
    public class JobCommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public JobCommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobComment(int jobId, [FromBody] CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Comment cannot be empty");

            var result = await _commentService.CreateForJobAsync(jobId, dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetJobComments(int jobId)
        {
            var comments = await _commentService.GetByJobIdAsync(jobId);
            return Ok(comments);
        }
    }
}
