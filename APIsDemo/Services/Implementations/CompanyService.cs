using APIsDemo.DTOs.Auth.Company;
using APIsDemo.DTOs.CompanyOverview;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Services.Implementations
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IEmailService _emailService;

        public CompanyService(AppDbContext context, JwtService jwt, IEmailService emailService)
        {
            _context = context;
            _jwt = jwt;
            _emailService = emailService;
        }

        public async Task RegisterAsync(RegisterCompanyDto dto)
        {
            if (await _context.Companies.AnyAsync(c => c.Email == dto.Email))
                throw new InvalidOperationException("Email already registered.");

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                if (await _context.CompanyOverviews.AnyAsync(o => o.Phone == dto.Phone))
                    throw new InvalidOperationException("Phone already registered.");
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
                PictureUrl = dto.PictureUrl,
                Overview = string.Empty
            };

            _context.CompanyOverviews.Add(overview);
            await _context.SaveChangesAsync();

            var otp = _emailService.GenerateOtp();
            company.Otp = otp;
            company.Otpexpiry = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync();

            await _emailService.SendOtpAsync(company.Email, otp);
        }

        public async Task<string> LoginAsync(LoginCompanyDto dto)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (company == null)
                throw new InvalidOperationException("Invalid email");

            if (!company.IsVerified)
                throw new InvalidOperationException("Email not verified. Please verify your email before login.");

            var hasher = new PasswordHasher<Company>();
            var verify = hasher.VerifyHashedPassword(company, company.PasswordHash, dto.Password);
            if (verify == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("Invalid password");

            var authorType = "Recruiter";
            var token = _jwt.GenerateToken(company.Id, authorType, company.Email);
            return token;
        }

        public async Task VerifyEmailAsync(VerifyCompanyEmailDto dto)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (company == null) throw new KeyNotFoundException("Company not found.");

            if (company.Otp != dto.Otp || company.Otpexpiry < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired OTP.");

            company.IsVerified = true;
            company.IsActive = true;
            company.Otp = null;
            company.Otpexpiry = null;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOverviewAsync(int companyId, UpdateOverviewDto dto)
        {
            var overview = await _context.CompanyOverviews.FirstOrDefaultAsync(o => o.CompanyId == companyId);

            if (overview == null)
            {
                overview = new CompanyOverview { CompanyId = companyId };
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
        }
    }
}
