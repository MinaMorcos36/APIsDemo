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
        private readonly IJobLikeService _jobLikeService;
        private readonly IJobSaveService _jobSaveService;

        public JobsController(IJobService jobService, IJobLikeService jobLikeService, IJobSaveService jobSaveService)
        {
            _jobService = jobService;
            _jobLikeService = jobLikeService;
            _jobSaveService = jobSaveService;
        }

        [Authorize(Policy = "RecruiterOnly")]
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

        [Authorize(Policy = "RecruiterOnly")]
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _jobService.GetJobCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetJobDetails(int jobId)
        {
            var job = await _jobService.GetJobDetailsAsync(jobId);
            return Ok(job);
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetJobs([FromQuery] string? filter, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var jobs = await _jobService.GetJobsAsync(filter, page, pageSize);
            return Ok(jobs);
        }

        [Authorize(Policy = "JobSeekerOnly")]
        [HttpPost("{jobId}/apply")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Apply(int jobId, [FromForm] ApplyJobDto dto, IFormFile? cvFile)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return BadRequest("Phone number is required.");

            // Validation: require exactly one of CvId or cvFile
            bool hasCvId = dto.CvId.HasValue;
            bool hasCvFile = cvFile != null && cvFile.Length > 0;

            if (hasCvId && hasCvFile)
                return BadRequest("Provide either a CV ID to select an existing CV or upload a new file, not both.");

            if (!hasCvId && !hasCvFile)
                return BadRequest("Either provide a CV ID to select an existing CV or upload a new CV file.");

            await _jobService.ApplyAsync(jobId, dto, cvFile);
            return Ok(new { Message = "Application submitted." });
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpGet("applications/{jobId}")]
        public async Task<IActionResult> GetApplications(int jobId, [FromQuery] string? filter, [FromQuery] string? sort, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var apps = await _jobService.GetApplicationsAsync(jobId, sort, page, pageSize);
            return Ok(apps);
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpGet("applications/{id}/cv")]
        public async Task<IActionResult> DownloadCv(int id)
        {
            var (content, fileName, contentType) = await _jobService.GetApplicationCvFileAsync(id);
            return File(content, contentType, fileName);
        }

        [Authorize(Policy = "JobSeekerOnly")]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications([FromQuery] string? filter, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var apps = await _jobService.GetMyApplicationsAsync(filter, page, pageSize);
            return Ok(apps);
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpPost("applications/{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            await _jobService.AcceptApplicationAsync(id);
            return Ok(new { Message = "Application accepted." });
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpPost("applications/{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            await _jobService.RejectApplicationAsync(id);
            return Ok(new { Message = "Application rejected." });
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpPost("{jobId}/set-active")]
        public async Task<IActionResult> SetActive(int jobId, [FromBody] SetActiveDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            var job = await _jobService.SetActiveAsync(jobId, dto.IsActive);
            return Ok(job);
        }

        [Authorize]
        [HttpPost("{jobId}/like")]
        public async Task<IActionResult> ToggleLike(int jobId)
        {
            var liked = await _jobLikeService.ToggleLikeAsync(jobId);

            return Ok(new
            {
                JobId = jobId,
                Liked = liked
            });
        }

        [Authorize]
        [HttpPost("{jobId}/save")]
        public async Task<IActionResult> ToggleSave(int jobId)
        {
            var saved = await _jobSaveService.ToggleSaveAsync(jobId);

            return Ok(new
            {
                JobId = jobId,
                Saved = saved
            });
        }

    }
}
