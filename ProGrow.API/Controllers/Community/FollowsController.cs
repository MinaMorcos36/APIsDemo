using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Community;

namespace ProGrow.API.Controllers.Community
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowsController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowsController(IFollowService followService)
        {
            _followService = followService;
        }

        // Toggle user->user follow (matches save toggle style)
        [HttpPost("user/{targetUserId}/follow")]
        [Authorize(Policy = "JobSeekerOnly")]
        public async Task<IActionResult> ToggleFollowUser([FromRoute] int targetUserId)
        {
            var followed = await _followService.ToggleUserFollowAsync(targetUserId);
            return Ok(new { UserId = targetUserId, Followed = followed });
        }

        // Toggle user->company follow
        [HttpPost("company/{targetCompanyId}/follow")]
        [Authorize(Policy = "JobSeekerOnly")]
        public async Task<IActionResult> ToggleFollowCompany([FromRoute] int targetCompanyId)
        {
            var followed = await _followService.ToggleCompanyFollowAsync(targetCompanyId);
            return Ok(new { CompanyId = targetCompanyId, Followed = followed });
        }

        // Toggle company->user follow (Recruiter)
        [HttpPost("company/{companyId}/follow/user/{targetUserId}")]
        [Authorize(Policy = "RecruiterOnly")]
        public async Task<IActionResult> ToggleCompanyFollowUser([FromRoute] int companyId, [FromRoute] int targetUserId)
        {
            var authorId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            if (authorId != companyId) return Forbid();

            var followed = await _followService.ToggleCompanyFollowUserAsync(targetUserId);
            return Ok(new { TargetUserId = targetUserId, Followed = followed });
        }

        // Toggle company->company follow (Recruiter)
        [HttpPost("company/{companyId}/follow/company/{targetCompanyId}")]
        [Authorize(Policy = "RecruiterOnly")]
        public async Task<IActionResult> ToggleCompanyFollowCompany([FromRoute] int companyId, [FromRoute] int targetCompanyId)
        {
            var authorId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            if (authorId != companyId) return Forbid();

            var followed = await _followService.ToggleCompanyFollowCompanyAsync(targetCompanyId);
            return Ok(new { TargetCompanyId = targetCompanyId, Followed = followed });
        }

        // Get followers and followings counts for a user profile card
        [HttpGet("profile/user/{userId}/counts")]
        public async Task<IActionResult> GetUserProfileCounts([FromRoute] int userId)
        {
            var counts = await _followService.GetUserProfileCountsAsync(userId);
            return Ok(counts);
        }

        // Get followers and followings counts for a company overview card
        [HttpGet("overview/company/{companyId}/counts")]
        public async Task<IActionResult> GetCompanyOverviewCounts([FromRoute] int companyId)
        {
            var counts = await _followService.GetCompanyOverviewCountsAsync(companyId);
            return Ok(counts);
        }
    }
}
