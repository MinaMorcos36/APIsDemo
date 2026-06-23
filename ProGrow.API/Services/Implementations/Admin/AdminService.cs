using ProGrow.API.DTOs.Admin.Dashboard;
using ProGrow.API.DTOs.Admin;

using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;

namespace ProGrow.API.Services.Implementations.Admin
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetCompaniesAsync()
        {
            var companies = await _context.Companies
                .AsNoTracking()
                .Where(c => !c.IsActive && c.IsVerified && !c.IsDeclined)
                .Select(c => new CompanyAdminDto
                {
                    Id = c.Id,
                    Email = c.Email,
                    IsVerified = c.IsVerified,
                    IsActive = c.IsActive,
                    Name = c.CompanyOverviews.Select(o => o.Name).FirstOrDefault(),
                    Phone = c.CompanyOverviews.Select(o => o.Phone).FirstOrDefault(),
                    Address = c.CompanyOverviews.Select(o => o.Address).FirstOrDefault(),
                    WebsiteUrl = c.CompanyOverviews.Select(o => o.WebsiteUrl).FirstOrDefault(),
                    PictureUrl = c.CompanyOverviews.Select(o => o.PictureUrl).FirstOrDefault()
                })
                .ToListAsync();

            return new OkObjectResult(companies);
        }

        public async Task<IActionResult> ApproveCompanyAsync(int companyId)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null) return new NotFoundObjectResult("Company not found.");

            if (company.IsActive)
                return new BadRequestObjectResult("Company is already activated.");

            company.IsActive = true;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Company approved successfully.");
        }

        public async Task<IActionResult> DeclineCompanyAsync(int companyId)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null) return new NotFoundObjectResult("Company not found.");

            if (!company.IsVerified && !company.IsActive)
                return new BadRequestObjectResult("Company is already declined or inactive.");

            company.IsActive = false;
            company.IsDeclined = true;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Company declined successfully.");
        }

        public async Task<IActionResult> GetSkillsAsync()
        {
            var skills = await _context.Skills
                .AsNoTracking()
                .Select(s => new SkillDto { Id = s.Id, Name = s.Name })
                .ToListAsync();

            return new OkObjectResult(skills);
        }

        public async Task<IActionResult> PostSkillsAsync(PostSkillsDto dto)
        {
            if (dto == null || dto.Names == null || !dto.Names.Any())
                return new BadRequestObjectResult("No skills provided.");

            var normalized = dto.Names
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // fetch DB skill names and compare in-memory with case-insensitive comparer
            var allDbNames = await _context.Skills
                .Select(s => s.Name)
                .ToListAsync();

            var existing = allDbNames
                .Where(dbName => normalized.Contains(dbName, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var toAdd = normalized.Except(existing, StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var name in toAdd)
            {
                _context.Skills.Add(new Skill { Name = name });
            }

            if (toAdd.Any())
                await _context.SaveChangesAsync();

            return new OkObjectResult(new { Added = toAdd, AlreadyExist = existing });
        }

        public async Task<IActionResult> DeleteSkillAsync(int skillId)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == skillId);
            if (skill == null) return new NotFoundObjectResult("Skill not found.");

            var userSkills = await _context.UserSkills
                .Where(us => us.SkillId == skillId)
                .ToListAsync();

            if (userSkills.Any())
                _context.UserSkills.RemoveRange(userSkills);

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Skill deleted successfully.");
        }

        public async Task<IActionResult> GetDashboardAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalCompanies = await _context.Companies.CountAsync();
            var totalJobs = await _context.Jobs.CountAsync();
            var totalApplications = await _context.JobApplications.CountAsync();

            var approvedCompanies = await _context.Companies.CountAsync(c => c.IsActive == true);

            var pendingCompanies = await _context.Companies.CountAsync(
                c => c.IsActive == false && c.IsDeclined == false
            );

            var rejectedCompanies = await _context.Companies.CountAsync(
                c => c.IsDeclined == true
            );
                var applicationStatuses = await _context.JobApplicationStatuses
        .Select(status => new StatusCountDto
        {
            Name = status.Name!,
            Count = status.JobApplications.Count()
        })
        .ToListAsync();

            var last7Days = DateTime.UtcNow.Date.AddDays(-6);

            var jobsData = await _context.Jobs
                .Where(j => j.CreatedAt >= last7Days)
                .GroupBy(j => j.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var jobsTrend = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var day = last7Days.AddDays(i);

                    return new TrendDto
                    {
                        Label = day.ToString("ddd"),
                        Count = jobsData
                            .FirstOrDefault(x => x.Date == day)?.Count ?? 0
                    };
                })
                .ToList();
            var applicationsData = await _context.JobApplications
                .Where(a => a.CreatedAt.HasValue && a.CreatedAt.Value.Date >= last7Days)
                .GroupBy(a => a.CreatedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var applicationsTrend = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var day = last7Days.AddDays(i);

                    return new TrendDto
                    {
                        Label = day.ToString("ddd"),
                        Count = applicationsData
                                .FirstOrDefault(x => x.Date == day)?.Count ?? 0
                    };
                })
                .ToList();
            var recentJobs = await _context.Jobs
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .Select(j => new RecentJobDto
                {
                    JobId = j.Id,
                    Title = j.Title,
                    CreatedAt = j.CreatedAt
                })
                .ToListAsync();

            var topCompanies = await _context.Companies
    .Select(c => new TopCompanyDto
    {
        CompanyId = c.Id,

        Name = c.CompanyOverviews
            .Select(o => o.Name)
            .FirstOrDefault() ?? "Unknown",

        JobsCount = c.Jobs.Count(),

        ApplicationsCount = c.Jobs
            .SelectMany(j => j.JobApplications)
            .Count(),

        Status = c.IsDeclined
            ? "Rejected"
            : c.IsActive
                ? "Approved"
                : "Pending"
    })
    .OrderByDescending(c => c.ApplicationsCount)
    .Take(5)
    .ToListAsync();
            var dashboard = new DashboardDto
            {
                TotalUsers = totalUsers,
                TotalCompanies = totalCompanies,
                TotalJobs = totalJobs,
                TotalApplications = totalApplications,
                ApprovedCompanies = approvedCompanies,
                PendingCompanies = pendingCompanies,
                RejectedCompanies = rejectedCompanies,
                ApplicationStatuses = applicationStatuses,
                JobsTrend = jobsTrend,
                ApplicationsTrend = applicationsTrend,
                RecentJobs = recentJobs,
                TopCompanies = topCompanies,
            };

            return new OkObjectResult(dashboard);
        }
    }
}
