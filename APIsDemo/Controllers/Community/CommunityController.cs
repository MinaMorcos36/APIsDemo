using APIsDemo.Services.Interfaces.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIsDemo.Controllers.Community
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly ICommunityService _communityService;

        public CommunityController(ICommunityService communityService)
        {
            _communityService = communityService;
        }

        [HttpGet("feed")]
        [Authorize]
        public async Task<IActionResult> GetFeed()
        {
            var feed = await _communityService.GetFeedAsync();
            return Ok(feed);
        }
    }
}
