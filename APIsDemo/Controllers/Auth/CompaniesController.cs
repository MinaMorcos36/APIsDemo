using APIsDemo.DTOs;
using APIsDemo.DTOs.Auth.Company;
using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.DTOs.CompanyOverview;
using APIsDemo.DTOs.UserProfile;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace APIsDemo.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        #region Services: DBContext,JWT,EmailService
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IEmailService _emailService;

        public CompaniesController(AppDbContext context, JwtService jwt, IEmailService emailService)
        {
            _context = context;
            _jwt = jwt;
            _emailService = emailService;
        }
        #endregion

        #region Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterCompanyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Companies.AnyAsync(c => c.Email == dto.Email))
            {
                return BadRequest("Email already registered.");
            }
            if (await _context.UserProfiles.AnyAsync(c => c.Phone == dto.Phone))
            {
                return BadRequest("Phone already registered.");
            }

            var company = new Company
            {
                Email = dto.Email,
                IsVerified = false,
                IsActive = false
            };

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Company>();
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

            return Ok("Company registered! OTP sent to email.");

        }
        #endregion

        #region Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginCompanyDto dto)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == dto.Email);

            if (company == null)
                return Unauthorized("Invalid email");

            if (!company.IsVerified)
                return Unauthorized("Email not verified. Please verify your email before login.");

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Company>();
            var verifyResult = hasher.VerifyHashedPassword(company, company.PasswordHash, dto.Password);
            if (verifyResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
                return Unauthorized("Invalid password");

            var authorType = "Recruiter";
            var token = _jwt.GenerateToken(company.Id, authorType, company.Email);

            return Ok(new { Token = token });
        }
        #endregion

        #region VerifyEmail
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyCompanyEmailDto dto)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (company == null) return NotFound("Company not found.");

            if (company.Otp != dto.Otp || company.Otpexpiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP.");

            company.IsVerified = true;
            company.IsActive = true;
            company.Otp = null;
            company.Otpexpiry = null;
            await _context.SaveChangesAsync();

            return Ok("Email verified successfully!");
        }
        #endregion

        #region UpdateOverview
        [Authorize]
        [HttpPatch("me/overview")]
        public async Task<IActionResult> UpdateOverview([FromBody] UpdateOverviewDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var overview = await _context.CompanyOverviews
                .FirstOrDefaultAsync(o => o.CompanyId == userId);

            if (overview == null)
            {
                overview = new CompanyOverview
                {
                    CompanyId = userId
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

            return Ok("Overview updated successfully");
        }

        #endregion
    }
}
