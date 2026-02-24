using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.DTOs.UserProfile;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APIsDemo.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        #region Services: DBContext,JWT,EmailService
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IEmailService _emailService;

        public UsersController(AppDbContext context, JwtService jwt, IEmailService emailService)
        {
            _context = context;
            _jwt = jwt;
            _emailService = emailService;
        }

        private int GetAuthorId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
        #endregion

        #region Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Email already registered.");
            }
            if (await _context.UserProfiles.AnyAsync(u => u.Phone == dto.Phone))
            {
                return BadRequest("Phone already registered.");
            }

            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Password and ConfirmPassword do not match.");

            var user = new User
            {
                Email = dto.Email,
                IsVerified = false,
                IsActive = false
            };

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
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

            return Ok("User registered! OTP sent to email.");
        }
        #endregion

        #region Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized("Invalid email");

            if (!user.IsVerified)
                return Unauthorized("Email not verified. Please verify your email before login.");

            var hasher = new PasswordHasher<User>();
            var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid password");

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            const string authorType = "JobSeeker";

            var token = _jwt.GenerateToken(user.Id, authorType, user.Email, roles);

            return Ok(new { Token = token, Roles = roles });
        }
        #endregion

        #region DefaultAuthUser
        [Authorize]
        [HttpGet("default")]
        public IActionResult GetCurrentUser()
        {
            return Ok("You are authenticated!");
        }
        #endregion

        #region Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult SecretAdminArea()
        {
            return Ok("Only admins can see this.");
        }
        #endregion

        #region GoogleLogin
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback));
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme); // "Google"
        }
        #endregion

        #region GoogleCallback
        // 2) Google will redirect here after user signs-in/consents
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            // Ask the Google handler to authenticate the incoming request
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded || authenticateResult?.Principal == null)
                return BadRequest("Google authentication failed");

            var externalPrincipal = authenticateResult.Principal;

            // Extract useful claims
            var email = externalPrincipal.FindFirst(ClaimTypes.Email)?.Value
                        ?? externalPrincipal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var googleId = externalPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Google's unique id
            var name = externalPrincipal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
                return BadRequest("Google did not return an email.");

            // 3) Find local user or create
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    PasswordHash = "FakeSecurePassword123!",//Random
                    IsVerified = true,
                    IsActive = true
                };
                _context.Users.Add(user);
                
                await _context.SaveChangesAsync();
            }

            // Check if ExternalLogin exists
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

            // 6) Return token. Real apps often redirect to front-end with token in URL or cookie.
            return Ok(new { JWT = token });
        }
        #endregion

        #region VerifyEmail
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyUserEmailDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return NotFound("User not found.");

            if (user.Otp != dto.Otp || user.Otpexpiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP.");

            user.IsVerified = true;
            user.IsActive = true;
            user.Otp = null;
            user.Otpexpiry = null;
            await _context.SaveChangesAsync();

            return Ok("Email verified successfully!");
        }
        #endregion

        #region UpdateProfile
        [Authorize]
        [HttpPatch("me/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId
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

            return Ok("Profile updated successfully");
        }

        #endregion

        #region GetProfile
        [Authorize]
        [HttpGet("me/profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var profile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId
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

            return Ok(response);
        }
        #endregion

        #region Saved Posts
        [Authorize]
        [HttpGet("SavedPosts")]
        public async Task<IActionResult> GetSavedPosts()
        {
            var authorId = GetAuthorId();
            var savedPosts = await _context.PostSaves
                .Where(sp => sp.AuthorId == authorId)
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
            return Ok(savedPosts);
        }
        #endregion
    }
}
