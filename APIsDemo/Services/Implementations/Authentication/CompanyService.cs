using APIsDemo.DTOs;
using APIsDemo.DTOs.Auth.Company;
using APIsDemo.DTOs.CompanyOverview;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace APIsDemo.Services.Implementations.Authentication
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

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
            company.IsActive = true;
            company.Otp = null;
            company.Otpexpiry = null;
            await _context.SaveChangesAsync();

            return new OkObjectResult("Email verified successfully!");
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

        private bool ValidateModel(object dto)
        {
            return dto != null;
        }
    }
}
