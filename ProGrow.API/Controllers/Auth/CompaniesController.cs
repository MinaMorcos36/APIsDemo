using ProGrow.API.DTOs.Auth.Company;
using ProGrow.API.DTOs.CompanyOverview;
using ProGrow.API.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProGrow.API.Controllers.Auth
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] RegisterCompanyDto dto, IFormFile? photo)
        {
            return await _companyService.RegisterAsync(dto, photo);
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

        #region GetIndustries
        [AllowAnonymous]
        [HttpGet("industries")]
        public async Task<IActionResult> GetIndustries()
        {
            return await _companyService.GetIndustriesAsync();
        }
        #endregion

        #region GetOverview
        [Authorize]
        [HttpGet("me/overview")]
        public async Task<IActionResult> GetOverview()
        {
            return await _companyService.GetOverviewAsync();
        }
        #endregion

        #region GetCompanyPhoto
        [AllowAnonymous]
        [HttpGet("{companyId}/photo")]
        public async Task<IActionResult> GetCompanyPhoto([FromRoute] int companyId)
        {
            return await _companyService.GetCompanyPhotoAsync(companyId);
        }
        #endregion

        #region Saved Posts
        [Authorize]
        [HttpGet("SavedPosts")]
        public async Task<IActionResult> GetSavedPosts()
        {
            return await _companyService.GetSavedPostsAsync();
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

        #region UploadCompanyPhoto
        [Authorize]
        [HttpPost("me/overview/photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCompanyPhoto(IFormFile photo)
        {
            return await _companyService.UploadCompanyPhotoAsync(photo);
        }
        #endregion

        #region Logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            return await _companyService.LogoutAsync();
        }
        #endregion
    }
}
