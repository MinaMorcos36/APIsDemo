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
    }
}