using ProGrow.API.DTOs.Community.Jobs;
using ProGrow.API.Services.Interfaces.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ProGrow.API.Controllers.Community
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
        public async Task<IActionResult> GetJobs([FromQuery] string? filter)
        {
            var jobs = await _jobService.GetJobsAsync(filter);
            return Ok(jobs);
        }

        [HttpPost("{jobId}/apply")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Apply(int jobId, [FromForm] ApplyJobDto dto, IFormFile cvFile)
        {
            if (dto == null || cvFile == null || cvFile.Length == 0 || string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return BadRequest("CV file and phone number are required.");

            await _jobService.ApplyAsync(jobId, dto, cvFile);
            return Ok(new { Message = "Application submitted." });
        }

        [HttpGet("applications/{jobId}")]
        public async Task<IActionResult> GetApplications(int jobId, [FromQuery] string? filter, [FromQuery] string? sort)
        {
            var apps = await _jobService.GetApplicationsAsync(jobId, filter, sort);
            return Ok(apps);
        }

        [HttpGet("applications/{id}/cv")]
        public async Task<IActionResult> DownloadCv(int id)
        {
            var (content, fileName, contentType) = await _jobService.GetApplicationCvFileAsync(id);
            return File(content, contentType, fileName);
        }

        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications([FromQuery] string? filter)
        {
            var apps = await _jobService.GetMyApplicationsAsync(filter);
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
        public async Task<IActionResult> SetActive(int jobId, [FromBody] SetActiveDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            var job = await _jobService.SetActiveAsync(jobId, dto.IsActive);
            return Ok(job);
        }
    }
}
