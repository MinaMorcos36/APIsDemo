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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateJob([FromForm] CreateJobDto dto, IFormFile bannerImage)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Job title is required.");

            if (bannerImage == null || bannerImage.Length == 0)
                return BadRequest("Banner image is required.");

            var result = await _jobService.CreateAsync(dto, bannerImage);
            return Ok(result);
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var feed = await _jobService.GetFeedAsync(page, pageSize);
            return Ok(feed);
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetJobDetails(int jobId)
        {
            var job = await _jobService.GetJobDetailsAsync(jobId);
            return Ok(job);
        }

        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetJobs([FromQuery] string? filter, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var jobs = await _jobService.GetJobsAsync(filter, page, pageSize);
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
        public async Task<IActionResult> GetApplications(int jobId, [FromQuery] string? filter, [FromQuery] string? sort, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var apps = await _jobService.GetApplicationsAsync(jobId, sort, page, pageSize);
            return Ok(apps);
        }

        [HttpGet("applications/{id}/cv")]
        public async Task<IActionResult> DownloadCv(int id)
        {
            var (content, fileName, contentType) = await _jobService.GetApplicationCvFileAsync(id);
            return File(content, contentType, fileName);
        }

        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications([FromQuery] string? filter, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var apps = await _jobService.GetMyApplicationsAsync(filter, page, pageSize);
            return Ok(apps);
        }

        [HttpPost("applications/{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            await _jobService.AcceptApplicationAsync(id);
            return Ok(new { Message = "Application accepted." });
        }

        [HttpPost("applications/{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            await _jobService.RejectApplicationAsync(id);
            return Ok(new { Message = "Application rejected." });
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
