using ProGrow.API.DTOs.Community.Jobs;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using ProGrow.API.Services.Extensions;
using System.Security.Claims;
using ProGrow.API.Services.Implementations.AI;
using Microsoft.AspNetCore.Http;

namespace ProGrow.API.Services.Implementations.Community
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FileParsingService _fileParsingService;
        private readonly CvProcessingService _cvProcessingService;
        private readonly GeminiCvEvaluationService _geminiService;

        public JobService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            FileParsingService fileParsingService,
            CvProcessingService cvProcessingService,
            GeminiCvEvaluationService geminiService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _fileParsingService = fileParsingService;
            _cvProcessingService = cvProcessingService;
            _geminiService = geminiService;
        }

        private int GetAuthorId()
        {
            return int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private string GetAuthorType()
        {
            return _httpContextAccessor.HttpContext!.User.FindFirstValue("AuthorType")!;
        }

        public async Task<JobResponseDto> CreateAsync(CreateJobDto dto)
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
            {
                throw new UnauthorizedAccessException("Only companies (recruiters) can create jobs.");
            }

            var companyId = GetAuthorId();

            var job = new Job
            {
                CompanyId = companyId,
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return new JobResponseDto
            {
                Id = job.Id,
                CompanyId = job.CompanyId,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                CreatedAt = job.CreatedAt,
                IsActive = true
            };
        }

        public async Task<List<JobFeedDto>> GetFeedAsync(int? page = null, int? pageSize = null)
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var jobs = await _context.Jobs
                .AsNoTracking()
                .OrderByDescending(j => j.CreatedAt)
                .ApplyPaging(page, pageSize)
                .Select(j => new JobFeedDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    Location = j.Location,
                    CreatedAt = j.CreatedAt!.Value,

                    CompanyId = j.CompanyId,
                    CompanyName = _context.CompanyOverviews
                        .Where(co => co.CompanyId == j.CompanyId)
                        .Select(co => co.Name)
                        .FirstOrDefault() ?? string.Empty,

                    ApplicantsCount = j.JobApplications.Count,
                    CommentsCount = j.Comments.Count,
                    IsAppliedByMe = j.JobApplications.Any(a => a.ApplicantId == authorId),
                    IsActive = j.IsActive ?? true
                })
                .ToListAsync();

            return jobs;
        }

        public async Task<List<CompanysJobDto>> GetJobsAsync(string? filter = null, int? page = null, int? pageSize = null)
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();
            // Build base query over Job entities so we can optionally filter before projection
            var jobsQuery = _context.Jobs
                .AsNoTracking()
                .OrderByDescending(j => j.CreatedAt)
                .AsQueryable();

            // If the caller is a recruiter, only return jobs belonging to their company
            if (authorType == "Recruiter")
            {
                var companyId = GetAuthorId();
                jobsQuery = jobsQuery.Where(j => j.CompanyId == companyId);
            }

            // Apply filter (all/active/closed)
            jobsQuery = jobsQuery.ApplyJobFilter(filter);

            var jobs = await jobsQuery
                .Select(j => new CompanysJobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    Location = j.Location,
                    CreatedAt = j.CreatedAt!.Value,

                    CompanyId = j.CompanyId,
                    CompanyName = _context.CompanyOverviews
                        .Where(co => co.CompanyId == j.CompanyId)
                        .Select(co => co.Name)
                        .FirstOrDefault() ?? string.Empty,

                    ApplicantsCount = j.JobApplications.Count,
                    CommentsCount = j.Comments.Count,
                    IsActive = j.IsActive ?? true,
                    JobStatus = (j.IsActive ?? true) ? "Active" : "Canceled"
                })
                .ApplyPaging(page, pageSize)
                .ToListAsync();

            return jobs;

        }

        public async Task ApplyAsync(int jobId, ApplyJobDto dto, IFormFile cvFile)
        {
            var authorType = GetAuthorType();
            if (authorType == "Recruiter")
                throw new UnauthorizedAccessException("Only jobseekers can apply to jobs.");

            var applicantId = GetAuthorId();

            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            if (job.IsActive == false)
                throw new InvalidOperationException("Cannot apply to an inactive job.");

            var already = await _context.JobApplications.AnyAsync(a => a.JobId == jobId && a.ApplicantId == applicantId);
            if (already)
                throw new InvalidOperationException("You have already applied to this job.");

            var pendingStatus = await _context.JobApplicationStatuses.FirstOrDefaultAsync(s => s.Name == "Pending");
            if (pendingStatus == null)
            {
                pendingStatus = new JobApplicationStatus { Name = "Pending" };
                _context.JobApplicationStatuses.Add(pendingStatus);
                await _context.SaveChangesAsync();
            }

            string text;
            try
            {
                text = _fileParsingService.Parse(cvFile);
            }
            catch (NotSupportedException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs");
            Directory.CreateDirectory(uploadsRoot);
            var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(cvFile.FileName)}";
            var savedPath = Path.Combine(uploadsRoot, safeFileName);
            await using (var stream = new FileStream(savedPath, FileMode.Create))
            {
                await cvFile.CopyToAsync(stream);
            }

            var cv = await _cvProcessingService.Save(applicantId, cvFile.FileName, text);

            int? score = null;
            string? reason = null;
            try
            {
                var evaluation = await _geminiService.EvaluateAsync(cv.RawText, job.Description ?? string.Empty);
                score = evaluation.Score;
                reason = evaluation.Reason;
            }
            catch (Exception)
            {
                reason = "AI service is currently unavailable";
            }

            var application = new JobApplication
            {
                JobId = jobId,
                ApplicantId = applicantId,
                StatusId = pendingStatus.Id,
                CreatedAt = DateTime.UtcNow,
                CvFileName = cvFile.FileName,
                CvFilePath = savedPath,
                CvId = cv.Id,
                CvScore = score,
                CvScoreReason = reason,
                CoverLetter = dto.CoverLetter,
                PhoneNumber = dto.PhoneNumber,
                PortfolioLink = dto.PortfolioLink
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();
        }

        public async Task<List<JobApplicationDto>> GetApplicationsAsync(int jobId, string? filter = null, string? sort = null, int? page = null, int? pageSize = null)
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
                throw new UnauthorizedAccessException("Only recruiters can view job applications.");

            var companyId = GetAuthorId();

            var query = _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Job)
                .Include(a => a.Status)
                .AsQueryable();

            query = query.Where(a => a.Job.CompanyId == companyId && a.JobId == jobId);

            // Apply applications filter (all/pending/accepted/rejected)
            query = query.ApplyApplicationFilter(filter);

            query = sort switch
            {
                "score" => query.OrderByDescending(a => a.CvScore ?? 0).ThenByDescending(a => a.CreatedAt),
                _ => query.OrderByDescending(a => a.CreatedAt)
            };

            var list = await query
                .Select(a => new JobApplicationDto
                {
                    Id = a.Id,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    JobDescription = a.Job.Description,
                    JobLocation = a.Job.Location,
                    CompanyName = _context.CompanyOverviews
                        .Where(co => co.CompanyId == a.Job.CompanyId)
                        .Select(co => co.Name)
                        .FirstOrDefault() ?? string.Empty,
                    ApplicantId = a.ApplicantId,
                    ApplicantName = (_context.UserProfiles
                        .Where(p => p.UserId == a.ApplicantId)
                        .Select(p => (p.FirstName ?? "") + " " + (p.LastName ?? ""))
                        .FirstOrDefault() ?? string.Empty)
                        .Trim(),
                    ApplicantHeadline = _context.UserProfiles
                        .Where(p => p.UserId == a.ApplicantId)
                        .Select(p => p.Headline)
                        .FirstOrDefault() ?? string.Empty,
                    ApplicantEmail = _context.Users.Where(u => u.Id == a.ApplicantId).Select(u => u.Email).FirstOrDefault()!,
                    CvId = a.CvId,
                    CvFileName = a.CvFileName,
                    CvScore = a.CvScore,
                    CvScoreReason = a.CvScoreReason,
                    CoverLetter = a.CoverLetter,
                    PhoneNumber = a.PhoneNumber,
                    PortfolioLink = a.PortfolioLink,
                    CreatedAt = a.CreatedAt!.Value,
                    StatusId = a.StatusId,
                    StatusName = a.Status.Name ?? string.Empty
                })
                .ApplyPaging(page, pageSize)
                .ToListAsync();

            return list;
        }

        public async Task<(byte[] Content, string FileName, string ContentType)> GetApplicationCvFileAsync(int applicationId)
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
                throw new UnauthorizedAccessException("Only recruiters can download CVs.");

            var companyId = GetAuthorId();

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Cv)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            if (application.Job.CompanyId != companyId)
                throw new UnauthorizedAccessException("You are not allowed to access this CV.");

            if (string.IsNullOrWhiteSpace(application.CvFilePath))
                throw new KeyNotFoundException("CV file not found.");

            if (!File.Exists(application.CvFilePath))
                throw new FileNotFoundException("CV file not found.");

            var fileName = Path.GetFileName(application.CvFilePath);
            var content = await File.ReadAllBytesAsync(application.CvFilePath);
            var contentType = GetContentType(fileName);

            return (content, fileName, contentType);
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        public async Task<List<JobApplicationDto>> GetMyApplicationsAsync(string? filter = null, int? page = null, int? pageSize = null)
        {
            var authorType = GetAuthorType();
            if (authorType == "Recruiter")
                throw new UnauthorizedAccessException("Only jobseekers can view their applications.");

            var applicantId = GetAuthorId();

            var list = await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Job)
                .Include(a => a.Status)
                .Where(a => a.ApplicantId == applicantId)
                .ApplyApplicationFilter(filter)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new JobApplicationDto
                {
                    Id = a.Id,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    JobDescription = a.Job.Description,
                    JobLocation = a.Job.Location,
                    CompanyName = _context.CompanyOverviews
                        .Where(co => co.CompanyId == a.Job.CompanyId)
                        .Select(co => co.Name)
                        .FirstOrDefault() ?? string.Empty,
                    ApplicantId = a.ApplicantId,
                    ApplicantName = (_context.UserProfiles
                        .Where(p => p.UserId == a.ApplicantId)
                        .Select(p => (p.FirstName ?? "") + " " + (p.LastName ?? ""))
                        .FirstOrDefault() ?? string.Empty)
                        .Trim(),
                    ApplicantHeadline = _context.UserProfiles
                        .Where(p => p.UserId == a.ApplicantId)
                        .Select(p => p.Headline)
                        .FirstOrDefault() ?? string.Empty,
                    ApplicantEmail = _context.Users.Where(u => u.Id == a.ApplicantId).Select(u => u.Email).FirstOrDefault()!,
                    CvId = a.CvId,
                    CvFileName = a.CvFileName,
                    CvScore = a.CvScore,
                    CvScoreReason = a.CvScoreReason,
                    CoverLetter = a.CoverLetter,
                    PhoneNumber = a.PhoneNumber,
                    PortfolioLink = a.PortfolioLink,
                    CreatedAt = a.CreatedAt!.Value,
                    StatusId = a.StatusId,
                    StatusName = a.Status.Name ?? string.Empty
                })
                .ApplyPaging(page, pageSize)
                .ToListAsync();

            return list;
        }

        public async Task ApproveApplicationAsync(int applicationId)
        {
            await UpdateApplicationStatus(applicationId, "Approved");
        }

        public async Task DeclineApplicationAsync(int applicationId)
        {
            await UpdateApplicationStatus(applicationId, "Declined");
        }

        private async Task UpdateApplicationStatus(int applicationId, string targetStatus)
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
                throw new UnauthorizedAccessException("Only recruiters can manage applications.");

            var companyId = GetAuthorId();

            var application = await _context.JobApplications.Include(a => a.Job).FirstOrDefaultAsync(a => a.Id == applicationId);
            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            if (application.Job.CompanyId != companyId)
                throw new UnauthorizedAccessException("You are not allowed to manage this application.");

            var status = await _context.JobApplicationStatuses.FirstOrDefaultAsync(s => s.Name == targetStatus);
            if (status == null)
            {
                status = new JobApplicationStatus { Name = targetStatus };
                _context.JobApplicationStatuses.Add(status);
                await _context.SaveChangesAsync();
            }

            application.StatusId = status.Id;
            application.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<JobResponseDto> SetActiveAsync(int jobId, bool isActive)
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
                throw new UnauthorizedAccessException("Only recruiters can change job active state.");

            var companyId = GetAuthorId();

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            if (job.CompanyId != companyId)
                throw new UnauthorizedAccessException("You are not allowed to modify this job.");

            job.IsActive = isActive;
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new JobResponseDto
            {
                Id = job.Id,
                CompanyId = job.CompanyId,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt,
                IsActive = isActive? true : false,
            };
        }
    }
}
