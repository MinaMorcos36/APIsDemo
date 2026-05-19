using ProGrow.API.DTOs.Auth.Company;
using ProGrow.API.DTOs.CompanyOverview;
using ProGrow.API.DTOs.Community;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

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

        public async Task<IActionResult> RegisterAsync(RegisterCompanyDto dto)
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
                PictureUrl = dto.PictureUrl
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

        public async Task<IActionResult> UpdateOverviewAsync(UpdateOverviewDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return new UnauthorizedResult();

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
                overview.IndustryId = dto.IndustryId;

            if (dto.Name != null)
                overview.Name = dto.Name;

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

            var response = new OverviewResponseDto
            {
                IndustryId = overview.IndustryId,
                Name = overview.Name,
                Phone = overview.Phone,
                Address = overview.Address,
                Overview = overview.Overview,
                WebsiteUrl = overview.WebsiteUrl,
                PictureUrl = overview.PictureUrl
            };

            return new OkObjectResult(response);
        }

        public async Task<IActionResult> GetSavedPostsAsync()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return new UnauthorizedResult();

            var savedPosts = await _context.PostSaves
                .Where(sp => sp.AuthorId == companyId.Value)
                .Select(sp => new SavedPostsDto
                {
                    SavedPostId = sp.Id,
                    SavedAt = sp.SavedAt,
                    PostId = sp.Post.Id,
                    Content = sp.Post.Content,
                    CreatedAt = sp.Post.CreatedAt,
                    AuthorId = sp.Post.AuthorId,
                    AuthorType = sp.Post.AuthorType
                })
                .ToListAsync();
            return new OkObjectResult(savedPosts);
        }

        private bool ValidateModel(object dto)
        {
            return dto != null;
        }
    }
}
