using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using ProGrow.API.DTOs.Auth.Company;
using ProGrow.API.DTOs.Community;
using ProGrow.API.DTOs.Community.Feed;
using ProGrow.API.DTOs.Community.Posts;
using ProGrow.API.DTOs.Community.Jobs;
using ProGrow.API.DTOs.CompanyOverview;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Authentication;
using System.Security.Claims;

namespace ProGrow.API.Services.Implementations.Authentication
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg"
        };
        private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;

        public CompanyService(AppDbContext context, JwtService jwt, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _jwt = jwt;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetCompanyId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var companyId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (companyId == null) return null;
            return int.Parse(companyId);
        }

        public async Task<IActionResult> RegisterAsync(RegisterCompanyDto dto, IFormFile? photo)
        {
            if (!ValidateModel(dto))
                return new BadRequestObjectResult("Invalid model");

            if (await _context.Companies.AnyAsync(c => c.Email == dto.Email))
            {
                return new BadRequestObjectResult("Email already registered.");
            }
            if (await _context.UserProfiles.AnyAsync(c => c.Phone == dto.Phone))
            {
                return new BadRequestObjectResult("Phone already registered.");
            }

            string? pictureUrl = null;
            if (photo != null)
            {
                if (photo.Length == 0)
                    return new BadRequestObjectResult("Photo is required.");

                if (photo.Length > MaxPhotoSizeBytes)
                    return new BadRequestObjectResult("Max file size is 5 MB.");

                var extension = Path.GetExtension(photo.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
                    return new BadRequestObjectResult("Invalid file type. Allowed: png, jpg, jpeg.");

                var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos", "companies");
                Directory.CreateDirectory(uploadsRoot);

                var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                var savedPath = Path.Combine(uploadsRoot, safeFileName);
                await using (var stream = new FileStream(savedPath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                pictureUrl = $"/uploads/photos/companies/{safeFileName}";
            }

            var company = new Company
            {
                Email = dto.Email,
                IsVerified = false,
                IsActive = false
            };

            var hasher = new PasswordHasher<Company>();
            company.PasswordHash = hasher.HashPassword(company, dto.Password);

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var overview = new CompanyOverview
            {
                CompanyId = company.Id,
                IndustryId = dto.IndustryId,
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                WebsiteUrl = dto.WebsiteUrl,
                PictureUrl = pictureUrl ?? dto.PictureUrl
            };

            _context.CompanyOverviews.Add(overview);
            await _context.SaveChangesAsync();

            var otp = _emailService.GenerateOtp();
            company.Otp = otp;
            company.Otpexpiry = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync();

            await _emailService.SendOtpAsync(company.Email, otp);

            return new OkObjectResult("Company registered! OTP sent to email.");
        }

        public async Task<IActionResult> LoginAsync(LoginCompanyDto dto)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == dto.Email);

            if (company == null)
                return new UnauthorizedObjectResult("Invalid email");

            if (!company.IsVerified)
                return new UnauthorizedObjectResult("Email not verified. Please verify your email before login.");
            
            if (!company.IsActive)
                return new UnauthorizedObjectResult("Email not activated. Please wait for admin approval.");

            var hasher = new PasswordHasher<Company>();
            var verifyResult = hasher.VerifyHashedPassword(company, company.PasswordHash, dto.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
                return new UnauthorizedObjectResult("Invalid password");

            var authorType = "Recruiter";
            var token = _jwt.GenerateToken(company.Id, authorType, company.Email);

            return new OkObjectResult(new { Token = token });
        }

        public async Task<IActionResult> VerifyEmailAsync(VerifyCompanyEmailDto dto)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (company == null) return new NotFoundObjectResult("Company not found.");

            if (company.Otp != dto.Otp || company.Otpexpiry < DateTime.UtcNow)
                return new BadRequestObjectResult("Invalid or expired OTP.");

            company.IsVerified = true;
            company.IsActive = false;
            company.Otp = null;
            company.Otpexpiry = null;
            await _context.SaveChangesAsync();

            return new OkObjectResult("Email verified successfully! Please Wait for admin approval to login.");
        }

        public async Task<IActionResult> GetIndustriesAsync()
        {
            var industries = await _context.Industries
                .AsNoTracking()
                .OrderBy(i => i.Name)
                .Select(i => new
                {
                    i.Id,
                    i.Name
                })
                .ToListAsync();

            return new OkObjectResult(industries);
        }

        public async Task<IActionResult> UpdateOverviewAsync(UpdateOverviewDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return new UnauthorizedResult();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId.Value);

            if (company == null)
                return new NotFoundObjectResult("Company not found.");

            var overview = await _context.CompanyOverviews
                .FirstOrDefaultAsync(o => o.CompanyId == companyId.Value);

            if (overview == null)
            {
                overview = new CompanyOverview
                {
                    CompanyId = companyId.Value
                };

                _context.CompanyOverviews.Add(overview);
                await _context.SaveChangesAsync();
            }

            if (dto.IndustryId != null)
            {
                var industryExists = await _context.Industries
                    .AnyAsync(i => i.Id == dto.IndustryId.Value);

                if (!industryExists)
                    return new BadRequestObjectResult("Invalid industry.");

                overview.IndustryId = dto.IndustryId;
            }

            if (dto.Name != null)
                overview.Name = dto.Name;

            if (dto.Email != null)
            {
                var normalizedEmail = dto.Email.Trim();
                if (string.IsNullOrWhiteSpace(normalizedEmail))
                    return new BadRequestObjectResult("Email is required.");

                var emailInUse = await _context.Companies
                    .AnyAsync(c => c.Email == normalizedEmail && c.Id != companyId.Value);

                if (emailInUse)
                    return new BadRequestObjectResult("Email already registered.");

                company.Email = normalizedEmail;
            }

            if (dto.Phone != null)
                overview.Phone = dto.Phone;

            if (dto.Address != null)
                overview.Address = dto.Address;

            if (dto.Overview != null)
                overview.Overview = dto.Overview;

            if (dto.WebsiteUrl != null)
                overview.WebsiteUrl = dto.WebsiteUrl;

            if (dto.PictureUrl != null)
                overview.PictureUrl = dto.PictureUrl;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Overview updated successfully");
        }

        public async Task<IActionResult> UploadCompanyPhotoAsync(IFormFile photo)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return new UnauthorizedResult();

            if (photo == null || photo.Length == 0)
                return new BadRequestObjectResult("Photo is required.");

            if (photo.Length > MaxPhotoSizeBytes)
                return new BadRequestObjectResult("Max file size is 5 MB.");

            var extension = Path.GetExtension(photo.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
                return new BadRequestObjectResult("Invalid file type. Allowed: png, jpg, jpeg.");

            var overview = await _context.CompanyOverviews
                .FirstOrDefaultAsync(o => o.CompanyId == companyId.Value);

            if (overview == null)
            {
                overview = new CompanyOverview
                {
                    CompanyId = companyId.Value
                };

                _context.CompanyOverviews.Add(overview);
                await _context.SaveChangesAsync();
            }

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos", "companies");
            Directory.CreateDirectory(uploadsRoot);

            var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var savedPath = Path.Combine(uploadsRoot, safeFileName);
            await using (var stream = new FileStream(savedPath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            if (!string.IsNullOrWhiteSpace(overview.PictureUrl))
            {
                DeleteExistingPhoto(overview.PictureUrl);
            }

            var relativeUrl = $"/uploads/photos/companies/{safeFileName}";
            overview.PictureUrl = relativeUrl;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { PictureUrl = relativeUrl });
        }

        public async Task<IActionResult> GetCompanyPhotoAsync(int companyId)
        {
            var pictureUrl = await _context.CompanyOverviews
                .AsNoTracking()
                .Where(o => o.CompanyId == companyId)
                .Select(o => o.PictureUrl)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(pictureUrl))
                return new NotFoundObjectResult("Photo not found.");

            return CreatePhotoResult(pictureUrl);
        }

        private IActionResult CreatePhotoResult(string pictureUrl)
        {
            var fullPath = BuildPhotoPath(pictureUrl);
            if (fullPath == null || !System.IO.File.Exists(fullPath))
                return new NotFoundObjectResult("Photo not found.");

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return new PhysicalFileResult(fullPath, contentType);
        }

        private void DeleteExistingPhoto(string pictureUrl)
        {
            var fullPath = BuildPhotoPath(pictureUrl);
            if (fullPath != null && System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
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

        public async Task<IActionResult> GetOverviewAsync()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return new UnauthorizedResult();

            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == companyId.Value);

            if (company == null)
                return new NotFoundObjectResult("Company not found.");

            var overview = await _context.CompanyOverviews
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.CompanyId == companyId.Value);

            if (overview == null)
            {
                overview = new CompanyOverview
                {
                    CompanyId = companyId.Value
                };

                _context.CompanyOverviews.Add(overview);
                await _context.SaveChangesAsync();
            }

            string? industryName = null;
            if (overview.IndustryId.HasValue)
            {
                industryName = await _context.Industries
                    .AsNoTracking()
                    .Where(i => i.Id == overview.IndustryId.Value)
                    .Select(i => i.Name)
                    .FirstOrDefaultAsync();
            }

            var response = new OverviewResponseDto
            {
                IndustryId = overview.IndustryId,
                IndustryName = industryName ?? string.Empty,
                Name = overview.Name,
                Email = company.Email,
                Phone = overview.Phone,
                Address = overview.Address,
                Overview = overview.Overview,
                WebsiteUrl = overview.WebsiteUrl,
                PictureUrl = overview.PictureUrl
            };

            return new OkObjectResult(response);
        }

        public async Task<IActionResult> GetSavedItemsAsync()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return new UnauthorizedResult();

            var authorId = companyId.Value;
            var authorType = "Recruiter";

            var postItems = await _context.PostSaves
                .Where(sp => sp.AuthorId == authorId && sp.AuthorType == authorType)
                .Select(sp => new FeedItemDto
                {
                    Type = "Post",
                    CreatedAt = sp.Post.CreatedAt ?? DateTime.UtcNow,
                    Post = new PostFeedDto
                    {
                        Id = sp.Post.Id,
                        Content = sp.Post.Content,
                        CreatedAt = sp.Post.CreatedAt ?? DateTime.UtcNow,
                        AuthorId = sp.Post.AuthorId,
                        AuthorType = sp.Post.AuthorType ?? string.Empty,
                        LikesCount = sp.Post.PostLikes.Count,
                        CommentsCount = sp.Post.Comments.Count,
                        IsLikedByMe = sp.Post.PostLikes.Any(l => l.AuthorId == authorId && l.AuthorType == authorType),
                        IsSavedByMe = true,
                        AuthorName = sp.Post.AuthorType == "JobSeeker"
                            ? _context.UserProfiles
                                .Where(up => up.UserId == sp.Post.AuthorId)
                                .Select(up => ((up.FirstName ?? string.Empty) + " " + (up.LastName ?? string.Empty)).Trim())
                                .FirstOrDefault() ?? string.Empty
                            : sp.Post.AuthorType == "Recruiter"
                                ? _context.CompanyOverviews
                                    .Where(co => co.CompanyId == sp.Post.AuthorId)
                                    .Select(co => co.Name)
                                    .FirstOrDefault() ?? string.Empty
                                : string.Empty,
                        AuthorPictureUrl = sp.Post.AuthorType == "JobSeeker"
                            ? _context.UserProfiles
                                .Where(up => up.UserId == sp.Post.AuthorId)
                                .Select(up => up.PictureUrl)
                                .FirstOrDefault()
                            : sp.Post.AuthorType == "Recruiter"
                                ? _context.CompanyOverviews
                                    .Where(co => co.CompanyId == sp.Post.AuthorId)
                                    .Select(co => co.PictureUrl)
                                    .FirstOrDefault()
                                : null,
                        AuthorSubtitle = sp.Post.AuthorType == "JobSeeker"
                            ? _context.UserProfiles
                                .Where(up => up.UserId == sp.Post.AuthorId)
                                .Select(up => up.Headline)
                                .FirstOrDefault()

                                : sp.Post.AuthorType == "Recruiter"
                                ? _context.CompanyOverviews
                                    .Where(co => co.CompanyId == sp.Post.AuthorId)
                                    .Select(co => co.Industry.Name)
                                    .FirstOrDefault()

                                : null,
                    }
                })
                .ToListAsync();

            var jobItems = await _context.JobSaves
                .Where(js => js.AuthorId == authorId && js.AuthorType == authorType)
                .Select(js => new FeedItemDto
                {
                    Type = "Job",
                    CreatedAt = js.Job.CreatedAt,
                    Job = new JobFeedDto
                    {
                        Id = js.Job.Id,
                        Title = js.Job.Title,
                        Description = js.Job.AboutRole,
                        Location = js.Job.CityOffice,
                        ShortDescription = js.Job.ShortDescription,
                        LocationMode = js.Job.LocationMode,
                        JobType = js.Job.JobType,
                        CityOffice = js.Job.CityOffice,
                        JobCategoryId = js.Job.JobCategoryId,
                        JobCategoryName = _context.JobCategories
                            .Where(c => c.Id == js.Job.JobCategoryId)
                            .Select(c => c.Name)
                            .FirstOrDefault() ?? string.Empty,
                        SalaryFrom = js.Job.SalaryFrom,
                        SalaryTo = js.Job.SalaryTo,
                        IsSalaryInInterview = js.Job.SalaryInInterview,
                        BannerImageUrl = js.Job.BannerImageUrl,
                        AboutRole = js.Job.AboutRole,
                        Responsibilities = js.Job.Responsibilities,
                        Requirements = js.Job.Requirements,
                        CreatedAt = js.Job.CreatedAt,
                        UpdatedAt = js.Job.UpdatedAt,

                        CompanyId = js.Job.CompanyId,
                        CompanyName = _context.CompanyOverviews
                            .Where(co => co.CompanyId == js.Job.CompanyId)
                            .Select(co => co.Name)
                            .FirstOrDefault() ?? string.Empty,
                        CompanyPictureUrl = _context.CompanyOverviews
                            .Where(co => co.CompanyId == js.Job.CompanyId)
                            .Select(co => co.PictureUrl)
                            .FirstOrDefault(),
                        CompanyIndustry = _context.CompanyOverviews
                            .Where(co => co.CompanyId == js.Job.CompanyId)
                            .Select(co => co.Industry.Name)
                            .FirstOrDefault(),

                        LikesCount = js.Job.JobLikes.Count,
                        SavesCount = js.Job.JobSaves.Count,
                        IsAppliedByMe = js.Job.JobApplications.Any(a => a.ApplicantId == authorId),
                        IsActive = js.Job.IsActive,
                        IsLikedByMe = js.Job.JobLikes.Any(l => l.AuthorId == authorId && l.AuthorType == authorType),
                        IsSavedByMe = true
                    }
                })
                .ToListAsync();

            var combined = postItems.Cast<FeedItemDto>().Concat(jobItems).OrderByDescending(i => i.CreatedAt).ToList();

            return new OkObjectResult(combined);
        }

        public Task<IActionResult> LogoutAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Task.FromResult<IActionResult>(new UnauthorizedResult());

            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            return Task.FromResult<IActionResult>(new OkObjectResult("Logged out successfully."));
        }

        private bool ValidateModel(object dto)
        {
            return dto != null;
        }
    }
}
