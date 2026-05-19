using ProGrow.API.DTOs.Admin;
using Microsoft.AspNetCore.Mvc;

namespace ProGrow.API.Services.Interfaces.Admin
{
    public interface IAdminService
    {
        Task<IActionResult> GetCompaniesAsync();
        Task<IActionResult> PostSkillsAsync(PostSkillsDto dto);
        Task<IActionResult> UpdateTaxAsync(UpdateTaxDto dto);
        Task<IActionResult> ApproveCompanyAsync(int companyId);
        Task<IActionResult> DeclineCompanyAsync(int companyId);
        Task<IActionResult> GetSkillsAsync();
        Task<IActionResult> DeleteSkillAsync(int skillId);
    }
}
