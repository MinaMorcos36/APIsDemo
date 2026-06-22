using ProGrow.API.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProGrow.API.Services.Interfaces.Admin;

namespace ProGrow.API.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            return await _adminService.GetCompaniesAsync();
        }

        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            return await _adminService.GetSkillsAsync();
        }

        [HttpPost("skills")]
        public async Task<IActionResult> PostSkills([FromBody] PostSkillsDto dto)
        {
            return await _adminService.PostSkillsAsync(dto);
        }

        [HttpPatch("companies/{id}/approve")]
        public async Task<IActionResult> ApproveCompany([FromRoute] int id)
        {
            return await _adminService.ApproveCompanyAsync(id);
        }

        [HttpPatch("companies/{id}/decline")]
        public async Task<IActionResult> DeclineCompany([FromRoute] int id)
        {
            return await _adminService.DeclineCompanyAsync(id);
        }

        [HttpDelete("skills/{id}")]
        public async Task<IActionResult> DeleteSkill([FromRoute] int id)
        {
            return await _adminService.DeleteSkillAsync(id);
        }
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            return await _adminService.GetDashboardAsync();
        }
    }
}