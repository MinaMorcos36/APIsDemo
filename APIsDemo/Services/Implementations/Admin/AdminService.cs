using APIsDemo.DTOs.Admin;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;

namespace APIsDemo.Services.Implementations.Admin
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetCompaniesAsync()
        {
            var companies = await _context.Companies
                .AsNoTracking()
                .Where(c => !c.IsVerified)
                .Select(c => new CompanyAdminDto
                {
                    Id = c.Id,
                    Email = c.Email,
                    IsVerified = c.IsVerified,
                    IsActive = c.IsActive,
                    Name = _context.CompanyOverviews.Where(o => o.CompanyId == c.Id).Select(o => o.Name).FirstOrDefault(),
                    Phone = _context.CompanyOverviews.Where(o => o.CompanyId == c.Id).Select(o => o.Phone).FirstOrDefault(),
                    WebsiteUrl = _context.CompanyOverviews.Where(o => o.CompanyId == c.Id).Select(o => o.WebsiteUrl).FirstOrDefault()
                })
                .ToListAsync();

            return new OkObjectResult(companies);
        }

        public async Task<IActionResult> ApproveCompanyAsync(int companyId)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null) return new NotFoundObjectResult("Company not found.");

            if (company.IsVerified)
                return new BadRequestObjectResult("Company is already verified.");

            company.IsVerified = true;
            company.IsActive = true;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Company approved successfully.");
        }

        public async Task<IActionResult> DeclineCompanyAsync(int companyId)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null) return new NotFoundObjectResult("Company not found.");

            if (!company.IsVerified && !company.IsActive)
                return new BadRequestObjectResult("Company is already declined or inactive.");

            company.IsVerified = false;
            company.IsActive = false;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Company declined successfully.");
        }

        public async Task<IActionResult> GetSkillsAsync()
        {
            var skills = await _context.Skills
                .AsNoTracking()
                .Select(s => new SkillDto { Id = s.Id, Name = s.Name })
                .ToListAsync();

            return new OkObjectResult(skills);
        }

        public async Task<IActionResult> PostSkillsAsync(PostSkillsDto dto)
        {
            if (dto == null || dto.Names == null || !dto.Names.Any())
                return new BadRequestObjectResult("No skills provided.");

            var normalized = dto.Names
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // fetch DB skill names and compare in-memory with case-insensitive comparer
            var allDbNames = await _context.Skills
                .Select(s => s.Name)
                .ToListAsync();

            var existing = allDbNames
                .Where(dbName => normalized.Contains(dbName, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var toAdd = normalized.Except(existing, StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var name in toAdd)
            {
                _context.Skills.Add(new Skill { Name = name });
            }

            if (toAdd.Any())
                await _context.SaveChangesAsync();

            return new OkObjectResult(new { Added = toAdd, AlreadyExist = existing });
        }

        public async Task<IActionResult> UpdateTaxAsync(UpdateTaxDto dto)
        {
            if (dto == null) return new BadRequestObjectResult("Invalid payload.");

            var tax = await _context.TaxSettings.FirstOrDefaultAsync();
            if (tax == null)
            {
                tax = new TaxSetting { Percentage = dto.Percentage };
                _context.TaxSettings.Add(tax);
            }
            else
            {
                tax.Percentage = dto.Percentage;
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult("Tax updated successfully");
        }
    }
}
