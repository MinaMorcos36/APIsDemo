using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.DTOs.UserProfile;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APIsDemo.Services.Implementations.Authentication
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(AppDbContext context, JwtService jwt, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _jwt = jwt;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetAuthorId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var id = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null) return null;
            return int.Parse(id);
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

        public async Task<IActionResult> GetCurrentUserAsync()
        {
            return new OkObjectResult("You are authenticated!");
        }

        public async Task<IActionResult> SecretAdminAreaAsync()
        {
            return new OkObjectResult("Only admins can see this.");
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

            if (dto.CvUrl != null)
                profile.Cvurl = dto.CvUrl;

            if (dto.FirstName != null)
                profile.FirstName = dto.FirstName;

            if (dto.LastName != null)
                profile.LastName = dto.LastName;

            if (dto.Phone != null)
                profile.Phone = dto.Phone;

            if (dto.Birthdate != null)
                profile.Birthdate = dto.Birthdate;

            if (dto.Address != null)
                profile.Address = dto.Address;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Profile updated successfully");
        }

        public async Task<IActionResult> GetProfileAsync()
        {
            var userId = GetAuthorId();
            if (userId == null) return new UnauthorizedResult();

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
                CvUrl = profile.Cvurl,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Phone = profile.Phone,
                Birthdate = profile.Birthdate,
                Address = profile.Address
            };

            return new OkObjectResult(response);
        }

        public async Task<IActionResult> GetSavedPostsAsync()
        {
            var authorId = GetAuthorId();
            if (authorId == null) return new UnauthorizedResult();

            var savedPosts = await _context.PostSaves
                .Where(sp => sp.AuthorId == authorId.Value)
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
            // minimal placeholder: controller previously relied on ModelState. Keep simple.
            return dto != null;
        }
    }
}
