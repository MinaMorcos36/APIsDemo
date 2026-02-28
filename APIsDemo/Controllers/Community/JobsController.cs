using APIsDemo.DTOs.Community.Jobs;
using APIsDemo.Services.Implementations;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIsDemo.Controllers.Community
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        private int GetAuthorId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        #region Create Job
        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Description) || string.IsNullOrWhiteSpace(dto.Location))
            {
                return BadRequest("Required fields cannot be empty.");
            }

            var authorId = GetAuthorId();

            var result = await _jobService.CreateAsync(dto, authorId);
            return Ok(result);
        }
        #endregion
    }
}
