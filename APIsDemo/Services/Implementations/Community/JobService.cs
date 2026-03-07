using APIsDemo.DTOs.Community.Jobs;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIsDemo.Services.Implementations.Community
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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
                CreatedAt = job.CreatedAt
            };
        }

        public async Task<List<JobFeedDto>> GetFeedAsync()
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var jobs = await _context.Jobs
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new JobFeedDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    Location = j.Location,
                    CreatedAt = j.CreatedAt!.Value,

                    CompanyId = j.CompanyId,
                    CompanyName = null!,

                    ApplicantsCount = j.JobApplications.Count,
                    IsAppliedByMe = j.JobApplications.Any(a => a.ApplicantId == authorId),
                    IsActive = j.IsActive ?? true
                })
                .ToListAsync();

            var companyIds = jobs.Select(j => j.CompanyId).Distinct().ToList();

            var overviews = await _context.CompanyOverviews
                .Where(co => companyIds.Contains(co.CompanyId))
                .Select(co => new { co.CompanyId, co.Name })
                .ToDictionaryAsync(x => x.CompanyId);

            foreach (var job in jobs)
            {
                if (overviews.TryGetValue(job.CompanyId, out var ov) && !string.IsNullOrWhiteSpace(ov.Name))
                {
                    job.CompanyName = ov.Name;
                }
                else
                {
                    job.CompanyName = string.Empty;
                }
            }

            return jobs;
        }

        public async Task ApplyAsync(int jobId)
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

            var application = new JobApplication
            {
                JobId = jobId,
                ApplicantId = applicantId,
                StatusId = pendingStatus.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DTOs.Community.Jobs.JobApplicationDto>> GetApplicationsAsync(int? jobId = null)
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
                throw new UnauthorizedAccessException("Only recruiters can view job applications.");

            var companyId = GetAuthorId();

            var query = _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Status)
                .AsQueryable();

            query = query.Where(a => a.Job.CompanyId == companyId);
            if (jobId.HasValue)
                query = query.Where(a => a.JobId == jobId.Value);

            var list = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new JobApplicationDto
                {
                    Id = a.Id,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    ApplicantId = a.ApplicantId,
                    ApplicantEmail = _context.Users.Where(u => u.Id == a.ApplicantId).Select(u => u.Email).FirstOrDefault()!,
                    CreatedAt = a.CreatedAt!.Value,
                    StatusId = a.StatusId,
                    StatusName = a.Status.Name ?? string.Empty
                })
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
                CreatedAt = job.CreatedAt
            };
        }
    }
}