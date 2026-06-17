using ProGrow.API.DTOs.Auth.JobSeeker;
using ProGrow.API.DTOs.Community;
using ProGrow.API.DTOs.Community.Feed;
using ProGrow.API.DTOs.Community.Posts;
using ProGrow.API.DTOs.Community.Jobs;
using ProGrow.API.DTOs.UserProfile;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace ProGrow.API.Services.Implementations.Authentication
{
    public class UserService : IUserService
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
        private readonly AI.CvProcessingService _cvService;
        private readonly AI.FileParsingService _fileParsingService;

        public UserService(AppDbContext context, JwtService jwt, IEmailService emailService, IHttpContextAccessor httpContextAccessor, AI.CvProcessingService cvService, AI.FileParsingService fileParsingService)
        {
            _context = context;
            _jwt = jwt;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _cvService = cvService;
            _fileParsingService = fileParsingService;
        }

        private int? GetAuthorId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var id = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null) return null;
            return int.Parse(id);
        }

        private string GetAuthorType()
        {
            return _httpContextAccessor.HttpContext!.User.FindFirstValue("AuthorType")!;
        }

        public async Task<IActionResult> RegisterAsync(RegisterUserDto dto)
        {
            if (!ValidateModel(dto))
                return new BadRequestObjectResult("Invalid model");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return new BadRequestObjectResult("Email already registered.");
            }
            if (await _context.UserProfiles.AnyAsync(u => u.Phone == dto.Phone))
            {
                return new BadRequestObjectResult("Phone already registered.");
            }

            if (dto.Password != dto.ConfirmPassword)
                return new BadRequestObjectResult("Password and ConfirmPassword do not match.");

            var user = new User
            {
                Email = dto.Email,
                IsVerified = false,
                IsActive = false
            };

            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new UserProfile
            {
                UserId = user.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Birthdate = dto.Birthdate,
                Phone = dto.Phone
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var otp = _emailService.GenerateOtp();
            user.Otp = otp;
            user.Otpexpiry = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync();

            await _emailService.SendOtpAsync(user.Email, otp);

            return new OkObjectResult("User registered! OTP sent to email.");
        }

        public async Task<IActionResult> LoginAsync(LoginUserDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return new UnauthorizedObjectResult("Invalid email");

            if (!user.IsVerified)
                return new UnauthorizedObjectResult("Email not verified. Please verify your email before login.");

            var hasher = new PasswordHasher<User>();
            var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
                return new UnauthorizedObjectResult("Invalid password");

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            const string authorType = "JobSeeker";

            var token = _jwt.GenerateToken(user.Id, authorType, user.Email, roles);

            return new OkObjectResult(new { Token = token, Roles = roles });
        }

        public async Task<IActionResult> GoogleCallbackAsync()
        {
            var http = _httpContextAccessor.HttpContext!;
            var authenticateResult = await http.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded || authenticateResult?.Principal == null)
                return new BadRequestObjectResult("Google authentication failed");

            var externalPrincipal = authenticateResult.Principal;

            var email = externalPrincipal.FindFirst(ClaimTypes.Email)?.Value
                        ?? externalPrincipal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var googleId = externalPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Google's unique id
            var name = externalPrincipal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
                return new BadRequestObjectResult("Google did not return an email.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    PasswordHash = "FakeSecurePassword123!",
                    IsVerified = true,
                    IsActive = true
                };
                _context.Users.Add(user);

                await _context.SaveChangesAsync();
            }

            var extLogin = await _context.ExternalLogins
                .FirstOrDefaultAsync(x => x.Provider == "Google" && x.ProviderKey == googleId);

            if (extLogin == null)
            {
                extLogin = new ExternalLogin
                {
                    UserId = user.Id,
                    Provider = "Google",
                    ProviderKey = googleId
                };
                _context.ExternalLogins.Add(extLogin);
                await _context.SaveChangesAsync();
            }

            var roles = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                where ur.UserId == user.Id
                select r.Name
            ).ToListAsync();

            var authorType = "JobSeeker";
            var token = _jwt.GenerateToken(user.Id, authorType, user.Email, roles);

            return new OkObjectResult(new { JWT = token });
        }

        public async Task<IActionResult> VerifyEmailAsync(VerifyUserEmailDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return new NotFoundObjectResult("User not found.");

            if (user.Otp != dto.Otp || user.Otpexpiry < DateTime.UtcNow)
                return new BadRequestObjectResult("Invalid or expired OTP.");

            user.IsVerified = true;
            user.IsActive = true;
            user.Otp = null;
            user.Otpexpiry = null;
            await _context.SaveChangesAsync();

            return new OkObjectResult("Email verified successfully!");
        }

        public async Task<IActionResult> UpdateProfileAsync(UpdateProfileDto dto)
        {
            var userId = GetAuthorId();
            if (userId == null) return new UnauthorizedResult();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
                return new NotFoundObjectResult("User not found.");

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId.Value
                };

                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            if (dto.Bio != null)
                profile.Bio = dto.Bio;

            if (dto.Headline != null)
                profile.Headline = dto.Headline;

            if (dto.Major != null)
                profile.Major = dto.Major;

            if (dto.University != null)
                profile.University = dto.University;

            if (dto.PictureUrl != null)
                profile.PictureUrl = dto.PictureUrl;

            if (dto.FirstName != null)
                profile.FirstName = dto.FirstName;

            if (dto.LastName != null)
                profile.LastName = dto.LastName;

            if (dto.Email != null)
            {
                var normalizedEmail = dto.Email.Trim();
                if (string.IsNullOrWhiteSpace(normalizedEmail))
                    return new BadRequestObjectResult("Email is required.");

                var emailInUse = await _context.Users
                    .AnyAsync(u => u.Email == normalizedEmail && u.Id != userId.Value);

                if (emailInUse)
                    return new BadRequestObjectResult("Email already registered.");

                user.Email = normalizedEmail;
            }

            if (dto.Phone != null)
                profile.Phone = dto.Phone;

            if (dto.Birthdate != null)
                profile.Birthdate = dto.Birthdate;

            if (dto.Address != null)
                profile.Address = dto.Address;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Profile updated successfully");
        }

        public async Task<IActionResult> UploadProfilePhotoAsync(IFormFile photo)
        {
            var userId = GetAuthorId();
            if (userId == null) return new UnauthorizedResult();

            if (photo == null || photo.Length == 0)
                return new BadRequestObjectResult("Photo is required.");

            if (photo.Length > MaxPhotoSizeBytes)
                return new BadRequestObjectResult("Max file size is 5 MB.");

            var extension = Path.GetExtension(photo.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
                return new BadRequestObjectResult("Invalid file type. Allowed: png, jpg, jpeg.");

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId.Value
                };

                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos", "profiles");
            Directory.CreateDirectory(uploadsRoot);

            var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var savedPath = Path.Combine(uploadsRoot, safeFileName);
            await using (var stream = new FileStream(savedPath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            if (!string.IsNullOrWhiteSpace(profile.PictureUrl))
            {
                DeleteExistingPhoto(profile.PictureUrl);
            }

            var relativeUrl = $"/uploads/photos/profiles/{safeFileName}";
            profile.PictureUrl = relativeUrl;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { PictureUrl = relativeUrl });
        }

        public async Task<IActionResult> UploadCvAsync(IFormFile cv)
        {
            var userId = GetAuthorId();
            if (userId == null) return new UnauthorizedResult();

            if (cv == null || cv.Length == 0)
                return new BadRequestObjectResult("CV file is required.");

            string text;
            try
            {
                text = _fileParsingService.Parse(cv);
            }
            catch (NotSupportedException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            var savedCv = await _cvService.Save(userId.Value, cv.FileName, text);
            return new OkObjectResult(new { Id = savedCv.Id, FileName = savedCv.FileName, Language = savedCv.Language, CreatedAt = savedCv.CreatedAt });
        }

        public async Task<IActionResult> GetUserPhotoAsync(int userId)
        {
            var pictureUrl = await _context.UserProfiles
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => p.PictureUrl)
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

        public async Task<IActionResult> GetProfileAsync()
        {
            var userId = GetAuthorId();
            if (userId == null) return new UnauthorizedResult();

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
                return new NotFoundObjectResult("User not found.");

            var profile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId.Value
                };

                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            var response = new ProfileResponseDto
            {
                Bio = profile.Bio,
                Headline = profile.Headline,
                Major = profile.Major,
                University = profile.University,
                PictureUrl = profile.PictureUrl,
                CvName = profile.CvName,
                CvScore = profile.CvScore,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Email = user.Email,
                Phone = profile.Phone,
                Birthdate = profile.Birthdate,
                Address = profile.Address
            };

            return new OkObjectResult(response);
        }

        public async Task<IActionResult> GetSavedItemsAsync()
        {
            var authorId = GetAuthorId();
            if (authorId == null) return new UnauthorizedResult();

            var authorType = GetAuthorType();

            var postItems = await _context.PostSaves
                .Where(sp => sp.AuthorId == authorId.Value && sp.AuthorType == authorType)
                .Select(sp => new FeedItemDto
                {
                    Type = "Post",
                    CreatedAt = sp.Post.CreatedAt ?? DateTime.UtcNow,
                    Post = new PostFeedDto
                    {
                        Id = sp.Post.Id,
                        Content = sp.Post.Content,
                        PostMediaUrl = sp.Post.PostMediaUrl,
                        CreatedAt = sp.Post.CreatedAt ?? DateTime.UtcNow,
                        AuthorId = sp.Post.AuthorId,
                        AuthorType = sp.Post.AuthorType ?? string.Empty,
                        LikesCount = sp.Post.PostLikes.Count,
                        CommentsCount = sp.Post.Comments.Count,
                        IsLikedByMe = sp.Post.PostLikes.Any(l => l.AuthorId == authorId.Value && l.AuthorType == authorType),
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
                .Where(js => js.AuthorId == authorId.Value && js.AuthorType == authorType)
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
                        IsAppliedByMe = js.Job.JobApplications.Any(a => a.ApplicantId == authorId.Value),
                        IsActive = js.Job.IsActive,
                        IsLikedByMe = js.Job.JobLikes.Any(l => l.AuthorId == authorId.Value && l.AuthorType == authorType),
                        IsSavedByMe = true
                    }
                })
                .ToListAsync();

            var combined = postItems.Cast<FeedItemDto>().Concat(jobItems).OrderByDescending(i => i.CreatedAt).ToList();
            return new OkObjectResult(combined);
        }

        public async Task<IActionResult> GetSkillsAsync()
        {
            var skills = await _context.Skills
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SkillSearchResultDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();

            return new OkObjectResult(skills);
        }

        public Task<IActionResult> LogoutAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Task.FromResult<IActionResult>(new UnauthorizedResult());

            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            return Task.FromResult<IActionResult>(new OkObjectResult("Logged out successfully."));
        }

        public async Task<IActionResult> RemoveSkillAsync(int skillId)
        {
            var userId = GetAuthorId();
            if (userId == null) return new UnauthorizedResult();

            if (skillId <= 0)
                return new BadRequestObjectResult("Invalid skill id.");

            var userSkill = await _context.UserSkills
                .FirstOrDefaultAsync(us => us.UserId == userId.Value && us.SkillId == skillId);

            if (userSkill == null)
                return new NotFoundObjectResult("Skill not found in profile.");

            _context.UserSkills.Remove(userSkill);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Skill removed successfully.");
        }

        public async Task<IActionResult> AddSkillAsync(AddUserSkillDto dto)
        {
            var userId = GetAuthorId();
            if (userId == null) return new UnauthorizedResult();

            if (dto == null || dto.SkillId <= 0)
                return new BadRequestObjectResult("Invalid payload.");

            var skillExists = await _context.Skills.AnyAsync(s => s.Id == dto.SkillId);
            if (!skillExists)
                return new NotFoundObjectResult("Skill not found.");

            var alreadyAdded = await _context.UserSkills
                .AnyAsync(us => us.UserId == userId.Value && us.SkillId == dto.SkillId);

            if (alreadyAdded)
                return new BadRequestObjectResult("Skill already added.");

            var levelId = await _context.SkillLevels
                .OrderBy(l => l.Id)
                .Select(l => (int?)l.Id)
                .FirstOrDefaultAsync();

            if (!levelId.HasValue)
                return new BadRequestObjectResult("No skill levels are configured.");

            var userSkill = new UserSkill
            {
                UserId = userId.Value,
                SkillId = dto.SkillId,
                LevelId = levelId.Value
            };

            _context.UserSkills.Add(userSkill);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Skill added successfully.");
        }

        public async Task<IActionResult> GetUserSkillsAsync()
        {
            var userId = GetAuthorId();
            if (userId == null) return new UnauthorizedResult();

            var skills = await _context.UserSkills
                .AsNoTracking()
                .Where(us => us.UserId == userId.Value)
                .Select(us => new UserSkillDto
                {
                    Id = us.Skill.Id,
                    Name = us.Skill.Name
                })
                .OrderBy(s => s.Name)
                .ToListAsync();

            return new OkObjectResult(skills);
        }

        private bool ValidateModel(object dto)
        {
            // minimal placeholder: controller previously relied on ModelState. Keep simple.
            return dto != null;
        }
    }
}
