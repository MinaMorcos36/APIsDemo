using ProGrow.API.DTOs;
using ProGrow.API.DTOs.Auth.Company;
using ProGrow.API.DTOs.CompanyOverview;
using Microsoft.AspNetCore.Mvc;

namespace ProGrow.API.Services.Interfaces.Authentication
{
    public interface ICompanyService
    {
        Task<IActionResult> RegisterAsync(RegisterCompanyDto dto, IFormFile? photo);
        Task<IActionResult> LoginAsync(LoginCompanyDto dto);
        Task<IActionResult> VerifyEmailAsync(VerifyCompanyEmailDto dto);
        Task<IActionResult> GetIndustriesAsync();
        Task<IActionResult> UpdateOverviewAsync(UpdateOverviewDto dto);
        Task<IActionResult> UploadCompanyPhotoAsync(IFormFile photo);
        Task<IActionResult> GetCompanyPhotoAsync(int companyId);
        Task<IActionResult> GetOverviewAsync();
        Task<IActionResult> GetSavedPostsAsync();
        Task<IActionResult> LogoutAsync();
    }
}
