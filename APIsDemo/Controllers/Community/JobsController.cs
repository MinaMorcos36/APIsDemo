using APIsDemo.DTOs.Community.Jobs;
using APIsDemo.DTOs.Community.Jobs;
using APIsDemo.Services.Interfaces.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Job title is required.");

            var result = await _jobService.CreateAsync(dto);
            return Ok(result);
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed()
        {
            var feed = await _jobService.GetFeedAsync();
            return Ok(feed);
        }

        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _jobService.GetJobsAsync();
            return Ok(jobs);
        }

        [HttpPost("{jobId}/apply")]
        public async Task<IActionResult> Apply(int jobId)
        {
            await _jobService.ApplyAsync(jobId);
            return Ok(new { Message = "Application submitted." });
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications([FromQuery] int? jobId)
        {
            var apps = await _jobService.GetApplicationsAsync(jobId);
            return Ok(apps);
        }

        [HttpPost("applications/{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            await _jobService.ApproveApplicationAsync(id);
            return Ok(new { Message = "Application approved." });
        }

        [HttpPost("applications/{id}/decline")]
        public async Task<IActionResult> Decline(int id)
        {
            await _jobService.DeclineApplicationAsync(id);
            return Ok(new { Message = "Application declined." });
        }

        [HttpPost("{jobId}/set-active")]
        public async Task<IActionResult> SetActive(int jobId, [FromQuery] bool isActive)
        {
            var job = await _jobService.SetActiveAsync(jobId, isActive);
            return Ok(job);
        }
    }
}