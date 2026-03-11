using APIsDemo.DTOs.Admin;
using Microsoft.AspNetCore.Mvc;

namespace APIsDemo.Services.Interfaces.Admin
{
    public interface IAdminService
    {
        Task<IActionResult> GetCompaniesAsync();
        Task<IActionResult> PostSkillsAsync(PostSkillsDto dto);
        Task<IActionResult> UpdateTaxAsync(UpdateTaxDto dto);
        Task<IActionResult> ApproveCompanyAsync(int companyId);
        Task<IActionResult> DeclineCompanyAsync(int companyId);
        Task<IActionResult> GetSkillsAsync();
    }
}
