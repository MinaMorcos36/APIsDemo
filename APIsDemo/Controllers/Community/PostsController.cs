using APIsDemo.DTOs.Community.Posts;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIsDemo.Controllers.Community
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

        private int GetAuthorId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private string GetAuthorType()
        {
            return User.FindFirstValue("AuthorType")!;
        }

        #region Create Post
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest("Post content cannot be empty.");
            }

            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var result = await _postService.CreateAsync(dto, authorId, authorType);
            return Ok(result);
        }
        #endregion

        #region Like Post
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var liked = await _postLikeService.ToggleLikeAsync(postId, authorId, authorType);

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
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var saved = await _SavePostService.ToggleSaveAsync(postId, authorId, authorType);

            return Ok(new
            {
                PostId = postId,
                Saved = saved
            });
        }
        #endregion

        #region Feed
        [HttpGet("feed")]
        [Authorize]
        public async Task<IActionResult> GetFeed()
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var feed = await _postService.GetFeedAsync(authorId, authorType);
            return Ok(feed);
        }

        #endregion
    }
}
