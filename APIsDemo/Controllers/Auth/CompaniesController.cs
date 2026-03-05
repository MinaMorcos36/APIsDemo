using APIsDemo.DTOs.Auth.Company;
using APIsDemo.DTOs.CompanyOverview;
using APIsDemo.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIsDemo.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        #region Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterCompanyDto dto)
        {
            return await _companyService.RegisterAsync(dto);
        }
        #endregion

        #region Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginCompanyDto dto)
        {
            return await _companyService.LoginAsync(dto);
        }
        #endregion

        #region VerifyEmail
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyCompanyEmailDto dto)
        {
            return await _companyService.VerifyEmailAsync(dto);
        }
        #endregion

        #region UpdateOverview
        [Authorize]
        [HttpPatch("me/overview")]
        public async Task<IActionResult> UpdateOverview([FromBody] UpdateOverviewDto dto)
        {
            return await _companyService.UpdateOverviewAsync(dto);
        }

        #endregion
    }
}
