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
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg"
        };
        private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;

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

        private static string? BuildPhotoPath(string pictureUrl)
        {
            var normalized = pictureUrl.Trim();
            if (!normalized.StartsWith("/", StringComparison.Ordinal))
            {
                normalized = "/" + normalized;
            }

            if (!normalized.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            normalized = normalized.TrimStart('/');
            normalized = normalized.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", normalized);
        }

        private static void DeleteExistingPhoto(string pictureUrl)
        {
            var fullPath = BuildPhotoPath(pictureUrl);
            if (fullPath != null && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public async Task<JobResponseDto> CreateAsync(CreateJobDto dto, IFormFile bannerImage)
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
            {
                throw new UnauthorizedAccessException("Only companies (recruiters) can create jobs.");
            }

            var companyId = GetAuthorId();

            if (dto.RequiredSkillIds.Any())
            {
                var skillIds = await _context.Skills
                    .Where(s => dto.RequiredSkillIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                if (skillIds.Count != dto.RequiredSkillIds.Distinct().Count())
                    throw new InvalidOperationException("One or more required skills are invalid.");
            }

            var categoryExists = await _context.JobCategories
                .AnyAsync(c => c.Id == dto.JobCategoryId);

            if (!categoryExists)
                throw new InvalidOperationException("Job category is invalid.");

            if (bannerImage == null || bannerImage.Length == 0)
                throw new InvalidOperationException("Banner image is required.");

            if (bannerImage.Length > MaxPhotoSizeBytes)
                throw new InvalidOperationException("Max file size is 5 MB.");

            var bannerExtension = Path.GetExtension(bannerImage.FileName);
            if (string.IsNullOrWhiteSpace(bannerExtension) || !AllowedImageExtensions.Contains(bannerExtension))
                throw new InvalidOperationException("Invalid file type. Allowed: png, jpg, jpeg.");

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos", "jobs");
            Directory.CreateDirectory(uploadsRoot);

            var safeFileName = $"{Guid.NewGuid():N}{bannerExtension.ToLowerInvariant()}";
            var savedPath = Path.Combine(uploadsRoot, safeFileName);
            await using (var stream = new FileStream(savedPath, FileMode.Create))
            {
                await bannerImage.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/photos/jobs/{safeFileName}";

            var job = new Job
            {
                CompanyId = companyId,
                Title = dto.Title,
                ShortDescription = dto.ShortDescription,
                JobTypeId = dto.JobTypeId,
                LocationModeId = dto.LocationModeId,
                CityOffice = dto.CityOffice,
                JobCategoryId = dto.JobCategoryId,
                SalaryFrom = dto.SalaryFrom,
                SalaryTo = dto.SalaryTo,
                SalaryInInterview = dto.IsSalaryInInterview,
                BannerImageUrl = relativeUrl,
                AboutRole = dto.AboutRole,
                Responsibilities = dto.Responsibilities,
                Requirements = dto.Requirements,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            if (dto.RequiredSkillIds.Any())
            {
                var jobSkills = dto.RequiredSkillIds
                    .Distinct()
                    .Select(skillId => new JobSkill
                    {
                        JobId = job.Id,
                        SkillId = skillId
                    })
                    .ToList();

                _context.JobSkills.AddRange(jobSkills);
                await _context.SaveChangesAsync();
            }

            return new JobResponseDto
            {
                Id = job.Id,
                CompanyId = job.CompanyId,
                Title = job.Title,
                ShortDescription = job.ShortDescription,
                LocationMode = _context.LocationModes.Where(l => l.Id == job.LocationModeId).Select(l => l.Name).FirstOrDefault() ?? string.Empty,
                CreatedAt = job.CreatedAt,
                IsActive = true,
                JobType = _context.JobTypes.Where(t => t.Id == job.JobTypeId).Select(t => t.Name).FirstOrDefault() ?? string.Empty,
                CityOffice = dto.CityOffice,
                JobCategoryId = job.JobCategoryId,
                JobCategoryName = _context.JobCategories
                    .Where(c => c.Id == job.JobCategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? string.Empty,
                SalaryFrom = dto.SalaryFrom,
                SalaryTo = dto.SalaryTo,
                IsSalaryInInterview = dto.IsSalaryInInterview,
                BannerImageUrl = job.BannerImageUrl,
                AboutRole = dto.AboutRole,
                Responsibilities = dto.Responsibilities,
                Requirements = dto.Requirements,
                RequiredSkills = await _context.Skills
                    .Where(s => dto.RequiredSkillIds.Contains(s.Id))
                    .Select(s => s.Name)
                    .ToListAsync()
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
                        Description = j.AboutRole,
                        Location = j.CityOffice,
                        ShortDescription = j.ShortDescription,
                        LocationMode = _context.LocationModes
                            .Where(l => l.Id == j.LocationModeId)
                            .Select(l => l.Name)
                            .FirstOrDefault() ?? string.Empty,
                        JobType = _context.JobTypes
                            .Where(t => t.Id == j.JobTypeId)
                            .Select(t => t.Name)
                            .FirstOrDefault() ?? string.Empty,
                        CityOffice = j.CityOffice,
                        JobCategoryId = j.JobCategoryId,
                        JobCategoryName = _context.JobCategories
                            .Where(c => c.Id == j.JobCategoryId)
                            .Select(c => c.Name)
                            .FirstOrDefault() ?? string.Empty,
                        RequiredSkills = j.JobSkills
                            .Select(js => js.Skill.Name)
                            .ToList(),
                        SalaryFrom = j.SalaryFrom,
                        SalaryTo = j.SalaryTo,
                        IsSalaryInInterview = j.SalaryInInterview,
                        BannerImageUrl = j.BannerImageUrl,
                        AboutRole = j.AboutRole,
                        Responsibilities = j.Responsibilities,
                        Requirements = j.Requirements,
                        CreatedAt = j.CreatedAt,
                        UpdatedAt = j.UpdatedAt,

                        CompanyId = j.CompanyId,
                        CompanyName = _context.CompanyOverviews
                            .Where(co => co.CompanyId == j.CompanyId)
                            .Select(co => co.Name)
                            .FirstOrDefault() ?? string.Empty,
                        CompanyPictureUrl = _context.CompanyOverviews
                            .Where(co => co.CompanyId == j.CompanyId)
                            .Select(co => co.PictureUrl)
                            .FirstOrDefault(),
                        CompanyIndustry = _context.CompanyOverviews
                            .Where(co => co.CompanyId == j.CompanyId)
                            .Select(co => co.Industry.Name)
                            .FirstOrDefault(),

                        LikesCount = j.JobLikes.Count,
                        SavesCount = j.JobSaves.Count,
                        IsFollowedByMe = authorType == "JobSeeker"
                            ? _context.UserFollows.Any(uf => uf.UserId == authorId && uf.FollowedCompanyId == j.CompanyId)
                            : _context.CompanyFollows.Any(cf => cf.CompanyId == authorId && cf.FollowedCompanyId == j.CompanyId),

                        ApplicantsCount = j.JobApplications.Count,
                        CommentsCount = j.Comments.Count,
                        IsAppliedByMe = j.JobApplications
                            .Any(a => a.ApplicantId == authorId),
                        IsActive = j.IsActive,
                        IsLikedByMe = j.JobLikes
                            .Any(l => l.AuthorId == authorId && l.AuthorType == authorType),
                        IsSavedByMe = j.JobSaves
                            .Any(s => s.AuthorId == authorId && s.AuthorType == authorType)
                })
                    .ToListAsync();

            return jobs;
        }

        public async Task<List<JobCategoryDto>> GetJobCategoriesAsync()
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
                throw new UnauthorizedAccessException("Only recruiters can view job categories.");

            return await _context.JobCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new JobCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<List<JobTypeDto>> GetJobTypesAsync()
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
                throw new UnauthorizedAccessException("Only recruiters can view job types.");

            return await _context.JobTypes
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new JobTypeDto
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToListAsync();
        }

        public async Task<List<LocationModeDto>> GetLocationModesAsync()
        {
            var authorType = GetAuthorType();
            if (authorType != "Recruiter")
                throw new UnauthorizedAccessException("Only recruiters can view location modes.");

            return await _context.LocationModes
                .AsNoTracking()
                .OrderBy(l => l.Name)
                .Select(l => new LocationModeDto
                {
                    Id = l.Id,
                    Name = l.Name
                })
                .ToListAsync();
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
                    Description = j.AboutRole,
                    Location = j.CityOffice,
                    ShortDescription = j.ShortDescription,
                    LocationMode = _context.LocationModes
                        .Where(l => l.Id == j.LocationModeId)
                        .Select(l => l.Name)
                        .FirstOrDefault() ?? string.Empty,
                    JobType = _context.JobTypes
                        .Where(t => t.Id == j.JobTypeId)
                        .Select(t => t.Name)
                        .FirstOrDefault() ?? string.Empty,
                    CityOffice = j.CityOffice,
                    JobCategoryId = j.JobCategoryId,
                    JobCategoryName = _context.JobCategories
                        .Where(c => c.Id == j.JobCategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? string.Empty,
                    SalaryFrom = j.SalaryFrom,
                    SalaryTo = j.SalaryTo,
                    IsSalaryInInterview = j.SalaryInInterview,
                    BannerImageUrl = j.BannerImageUrl,
                    AboutRole = j.AboutRole,
                    Responsibilities = j.Responsibilities,
                    Requirements = j.Requirements,
                    CreatedAt = j.CreatedAt,
                    UpdatedAt = j.UpdatedAt,

                    CompanyId = j.CompanyId,
                    CompanyName = _context.CompanyOverviews
                        .Where(co => co.CompanyId == j.CompanyId)
                        .Select(co => co.Name)
                        .FirstOrDefault() ?? string.Empty,
                    CompanyPictureUrl = _context.CompanyOverviews
                        .Where(co => co.CompanyId == j.CompanyId)
                        .Select(co => co.PictureUrl)
                        .FirstOrDefault(),

                    LikesCount = j.JobLikes.Count,
                    SavesCount = j.JobSaves.Count,
                    ApplicantsTotalCount = j.JobApplications.Count,
                    IsLikedByMe = j.JobLikes.Any(l => l.AuthorId == authorId && l.AuthorType == authorType),
                    IsSavedByMe = j.JobSaves.Any(s => s.AuthorId == authorId && s.AuthorType == authorType),
                    IsFollowedByMe = authorType == "JobSeeker"
                        ? _context.UserFollows.Any(uf => uf.UserId == authorId && uf.FollowedCompanyId == j.CompanyId)
                        : _context.CompanyFollows.Any(cf => cf.CompanyId == authorId && cf.FollowedCompanyId == j.CompanyId),
                    ApplicantAvatarUrls = j.JobApplications
                        .OrderByDescending(a => a.CreatedAt)
                        .Select(a => _context.UserProfiles
                            .Where(p => p.UserId == a.ApplicantId)
                            .Select(p => p.PictureUrl)
                            .FirstOrDefault())
                        .Where(url => url != null)
                        .Take(3)
                        .Select(url => url!)
                        .ToList(),

                    ApplicantsCount = j.JobApplications.Count,
                    CommentsCount = j.Comments.Count,
                    IsActive = j.IsActive,
                    JobStatus = j.IsActive ? "Active" : "Closed"
                })
                .ApplyPaging(page, pageSize)
                .ToListAsync();

            return jobs;

        }

        public async Task ApplyAsync(int jobId, ApplyJobDto dto, IFormFile? cvFile)
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

            CvModel cv;
            string cvFileName;
            string? cvFilePath = null;

            // Handle CV selection vs upload
            if (dto.CvId.HasValue)
            {
                // User selected an existing CV
                cv = await _context.Cvs.FirstOrDefaultAsync(c => c.Id == dto.CvId.Value && c.UserId == applicantId);
                if (cv == null)
                    throw new KeyNotFoundException("Selected CV not found or does not belong to you.");

                cvFileName = cv.FileName;
                cvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs", cv.FileName);
            }
            else if (cvFile != null && cvFile.Length > 0)
            {
                // User uploaded a new CV
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
                cvFilePath = Path.Combine(uploadsRoot, safeFileName);
                await using (var stream = new FileStream(cvFilePath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(stream);
                }

                cv = await _cvProcessingService.Save(applicantId, cvFile.FileName, text);
                cvFileName = cvFile.FileName;
            }
            else
            {
                throw new InvalidOperationException("Either provide a CV ID to select an existing CV or upload a new CV file.");
            }

            // Evaluate CV against job description
            int? score = null;
            string? reason = null;
            try
            {
                var evaluation = await _geminiService.EvaluateAsync(cv.RawText, job.AboutRole ?? string.Empty);
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
                CvFileName = cvFileName,
                CvFilePath = cvFilePath,
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

        public async Task<List<JobApplicationDto>> GetApplicationsAsync(int jobId, string? sort = null, int? page = null, int? pageSize = null)
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
                    JobDescription = a.Job.AboutRole,
                    JobLocation = a.Job.CityOffice,
                    JobShortDescription = a.Job.ShortDescription,
                    JobLocationMode = _context.LocationModes
                        .Where(l => l.Id == a.Job.LocationModeId)
                        .Select(l => l.Name)
                        .FirstOrDefault(),
                    JobType = _context.JobTypes
                        .Where(t => t.Id == a.Job.JobTypeId)
                        .Select(t => t.Name)
                        .FirstOrDefault(),
                    RequiredSkillsName = a.Job.JobSkills
                            .Select(js => js.Skill.Name)
                            .ToList(),
                    IsActive = a.Job.IsActive,
                    JobStatus = a.Job.IsActive ? "Active" : "Closed",
                    JobCityOffice = a.Job.CityOffice,
                    JobSalaryFrom = a.Job.SalaryFrom,
                    JobSalaryTo = a.Job.SalaryTo,
                    JobIsSalaryInInterview = a.Job.SalaryInInterview,
                    JobBannerImageUrl = a.Job.BannerImageUrl,
                    JobAboutRole = a.Job.AboutRole,
                    JobResponsibilities = a.Job.Responsibilities,
                    JobRequirements = a.Job.Requirements,
                    JobUpdatedAt = a.Job.UpdatedAt,
                    CompanyName = _context.CompanyOverviews
                        .Where(co => co.CompanyId == a.Job.CompanyId)
                        .Select(co => co.Name)
                        .FirstOrDefault() ?? string.Empty,
                    CompanyPictureUrl = _context.CompanyOverviews
                        .Where(co => co.CompanyId == a.Job.CompanyId)
                        .Select(co => co.PictureUrl)
                        .FirstOrDefault(),
                    ApplicantId = a.ApplicantId,    
                    ApplicantName = (_context.UserProfiles
                        .Where(p => p.UserId == a.ApplicantId)
                        .Select(p => (p.FirstName ?? "") + " " + (p.LastName ?? ""))
                        .FirstOrDefault() ?? string.Empty)
                        .Trim(),
                    ApplicantPictureUrl = _context.UserProfiles
                        .Where(p => p.UserId == a.ApplicantId)
                        .Select(p => p.PictureUrl)
                        .FirstOrDefault(),
                    ApplicantHeadline = _context.UserProfiles
                        .Where(p => p.UserId == a.ApplicantId)
                        .Select(p => p.Headline)
                        .FirstOrDefault() ?? string.Empty,
                    ApplicantEmail = _context.Users
                        .Where(u => u.Id == a.ApplicantId)
                        .Select(u => u.Email)
                        .FirstOrDefault()!,
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
                    JobDescription = a.Job.AboutRole,
                    JobLocation = a.Job.CityOffice,
                    JobShortDescription = a.Job.ShortDescription,
                    JobLocationMode = _context.LocationModes
                        .Where(l => l.Id == a.Job.LocationModeId)
                        .Select(l => l.Name)
                        .FirstOrDefault(),
                    JobType = _context.JobTypes
                        .Where(t => t.Id == a.Job.JobTypeId)
                        .Select(t => t.Name)
                        .FirstOrDefault(),
                    RequiredSkillsName = a.Job.JobSkills
                            .Select(js => js.Skill.Name)
                            .ToList(),
                    IsActive = a.Job.IsActive,
                    JobStatus = a.Job.IsActive ? "Active" : "Closed",
                    JobCityOffice = a.Job.CityOffice,
                    JobSalaryFrom = a.Job.SalaryFrom,
                    JobSalaryTo = a.Job.SalaryTo,
                    JobIsSalaryInInterview = a.Job.SalaryInInterview,
                    JobBannerImageUrl = a.Job.BannerImageUrl,
                    JobAboutRole = a.Job.AboutRole,
                    JobResponsibilities = a.Job.Responsibilities,
                    JobRequirements = a.Job.Requirements,
                    JobUpdatedAt = a.Job.UpdatedAt,
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
                    CompanyPictureUrl = _context.CompanyOverviews
                        .Where(co => co.CompanyId == a.Job.CompanyId)
                        .Select(co => co.PictureUrl)
                        .FirstOrDefault(),
                    ApplicantEmail = _context.Users
                        .Where(u => u.Id == a.ApplicantId)
                        .Select(u => u.Email)
                        .FirstOrDefault()!,
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

        public async Task AcceptApplicationAsync(int applicationId)
        {
            await UpdateApplicationStatus(applicationId, "Accepted");
        }

        public async Task RejectApplicationAsync(int applicationId)
        {
            await UpdateApplicationStatus(applicationId, "Rejected");
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
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt,
                IsActive = isActive,
                ShortDescription = job.ShortDescription,
                LocationMode = _context.LocationModes
                    .Where(l => l.Id == job.LocationModeId)
                    .Select(l => l.Name)
                    .FirstOrDefault() ?? string.Empty,
                JobType = _context.JobTypes
                    .Where(t => t.Id == job.JobTypeId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? string.Empty,
                CityOffice = job.CityOffice,
                JobCategoryId = job.JobCategoryId,
                JobCategoryName = _context.JobCategories
                    .Where(c => c.Id == job.JobCategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? string.Empty,
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                IsSalaryInInterview = job.SalaryInInterview,
                BannerImageUrl = job.BannerImageUrl,
                AboutRole = job.AboutRole,
                Responsibilities = job.Responsibilities,
                Requirements = job.Requirements,
                RequiredSkills = await _context.JobSkills
                    .Where(js => js.JobId == job.Id)
                    .Select(js => js.Skill.Name)
                    .ToListAsync()
            };
        }

        public async Task<JobDetailsDto> GetJobDetailsAsync(int jobId)
        {
            var job = await _context.Jobs
                .AsNoTracking()
                .Include(j => j.JobSkills)
                .ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            var companyOverview = await _context.CompanyOverviews
                .AsNoTracking()
                .FirstOrDefaultAsync(co => co.CompanyId == job.CompanyId);

            return new JobDetailsDto
            {
                Id = job.Id,
                CompanyId = job.CompanyId,
                CompanyName = companyOverview?.Name ?? string.Empty,
                CompanyPictureUrl = companyOverview?.PictureUrl,
                CityOffice = job.CityOffice,
                Title = job.Title,
                LocationMode = _context.LocationModes
                    .Where(l => l.Id == job.LocationModeId)
                    .Select(l => l.Name)
                    .FirstOrDefault() ?? string.Empty,
                JobType = _context.JobTypes
                    .Where(t => t.Id == job.JobTypeId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? string.Empty,
                JobCategoryId = job.JobCategoryId,
                JobCategoryName = _context.JobCategories
                    .Where(c => c.Id == job.JobCategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? string.Empty,
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                IsSalaryInInterview = job.SalaryInInterview,
                RequiredSkills = job.JobSkills
                    .Select(js => js.Skill.Name).ToList(),
                AboutRole = job.AboutRole,
                Responsibilities = job.Responsibilities,
                Requirements = job.Requirements,
            };
        }

        public async Task<bool> ToggleLikeAsync(int jobId)
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);
            if (!jobExists)
                throw new KeyNotFoundException("Job not found.");

            var existingLike = await _context.JobLikes
                .FirstOrDefaultAsync(jl => jl.JobId == jobId && jl.AuthorId == authorId && jl.AuthorType == authorType);

            if (existingLike != null)
            {
                _context.JobLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false;
            }

            var like = new JobLike
            {
                JobId = jobId,
                AuthorId = authorId,
                AuthorType = authorType,
                CreatedAt = DateTime.UtcNow
            };

            _context.JobLikes.Add(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleSaveAsync(int jobId)
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);
            if (!jobExists)
                throw new KeyNotFoundException("Job not found.");

            var existingSave = await _context.JobSaves
                .FirstOrDefaultAsync(js => js.JobId == jobId && js.AuthorId == authorId && js.AuthorType == authorType);

            if (existingSave != null)
            {
                _context.JobSaves.Remove(existingSave);
                await _context.SaveChangesAsync();
                return false;
            }

            var save = new JobSave
            {
                JobId = jobId,
                AuthorId = authorId,
                AuthorType = authorType,
                SavedAt = DateTime.UtcNow
            };

            _context.JobSaves.Add(save);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
