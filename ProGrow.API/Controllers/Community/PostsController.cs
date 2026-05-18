using ProGrow.API.DTOs.Community.Posts;
using ProGrow.API.Services.Interfaces.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProGrow.API.Controllers.Community
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IPostLikeService _postLikeService;
        private readonly IPostSaveService _SavePostService;

        public PostsController(IPostService postService, IPostLikeService postLikeService, IPostSaveService savePostService)
        {
            _postService = postService;
            _postLikeService = postLikeService;
            _SavePostService = savePostService;
        }

        #region Create Post
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest("Post content cannot be empty.");
            }

            var result = await _postService.CreateAsync(dto);
            return Ok(result);
        }
        #endregion

        #region Get Feed
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var feed = await _postService.GetFeedAsync(page, pageSize);
            return Ok(feed);
        }
        #endregion

        #region Like Post
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var liked = await _postLikeService.ToggleLikeAsync(postId);

            return Ok(new
            {
                PostId = postId,
                Liked = liked
            });
        }
        #endregion

        #region Save Post
        [HttpPost("{postId}/save")]
        public async Task<IActionResult> ToggleSave(int postId)
        {
            var saved = await _SavePostService.ToggleSaveAsync(postId);

            return Ok(new
            {
                PostId = postId,
                Saved = saved
            });
        }
        #endregion
    }
}
