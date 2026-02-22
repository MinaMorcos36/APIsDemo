using APIsDemo.DTOs;
using APIsDemo.DTOs.Auth.Company;
using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            if (await _context.Companies.AnyAsync(c => c.Email == dto.Email))
            {
                return BadRequest("Email already registered.");
            }

            var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(dto.Password));

            var company = new Company
            {
                Email = dto.Email,
                PasswordHash = passwordHash
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // 2. Generate OTP
            var otp = _emailService.GenerateOtp();
            company.Otp = otp;
            company.Otpexpiry = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync();

            // 3. Send OTP via email
            await _emailService.SendOtpAsync(company.Email, otp);

            return Ok("Company registered! OTP sent to email.");

        }
        #endregion

        #region Login
        [HttpPost("Login")]
        public IActionResult Login(LoginCompanyDto dto, JwtService jwt)
        {
            var company = _context.Companies
                .FirstOrDefault(c => c.Email == dto.Email);

            if (company == null)
                return Unauthorized("Invalid email");

            var passwordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(dto.Password));

            if (company.PasswordHash != passwordHash)
                return Unauthorized("Invalid password");

            var authorType = "Recruiter";
            var token = jwt.GenerateToken(company.Id, authorType, company.Email);

            return Ok(new { Token = token});

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
            company.Otp = null;
            company.Otpexpiry = null;
            await _context.SaveChangesAsync();

            return Ok("Email verified successfully!");
        }
        #endregion
    }
}
