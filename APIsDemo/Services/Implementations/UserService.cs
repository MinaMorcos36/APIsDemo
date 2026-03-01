using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.DTOs.UserProfile;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IEmailService _emailService;

        public UserService(AppDbContext context, JwtService jwt, IEmailService emailService)
        {
            _context = context;
            _jwt = jwt;
            _emailService = emailService;
        }

        public async Task RegisterAsync(RegisterUserDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new ArgumentException("Password and ConfirmPassword do not match.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already registered.");

            if (await _context.UserProfiles.AnyAsync(u => u.Phone == dto.Phone))
                throw new InvalidOperationException("Phone already registered.");

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
        }

        public async Task<(string Token, List<string> Roles)> LoginAsync(LoginUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                throw new InvalidOperationException("Invalid email");

            if (!user.IsVerified)
                throw new InvalidOperationException("Email not verified. Please verify your email before login.");

            var hasher = new PasswordHasher<User>();
            var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("Invalid password");

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            const string authorType = "JobSeeker";

            var token = _jwt.GenerateToken(user.Id, authorType, user.Email, roles);

            return (token, roles);
        }

        public async Task VerifyEmailAsync(VerifyUserEmailDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) throw new KeyNotFoundException("User not found.");

            if (user.Otp != dto.Otp || user.Otpexpiry < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired OTP.");

            user.IsVerified = true;
            user.IsActive = true;
            user.Otp = null;
            user.Otpexpiry = null;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
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
        }

        public async Task<ProfileResponseDto> GetProfileAsync(int userId)
        {
            var profile = await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return new ProfileResponseDto
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
        }
    }
}
