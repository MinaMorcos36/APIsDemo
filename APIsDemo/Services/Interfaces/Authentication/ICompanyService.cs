using APIsDemo.DTOs;
using APIsDemo.DTOs.Auth.Company;
using APIsDemo.DTOs.CompanyOverview;
using Microsoft.AspNetCore.Mvc;

namespace APIsDemo.Services.Interfaces.Authentication
{
    public interface ICompanyService
    {
        Task<IActionResult> RegisterAsync(RegisterCompanyDto dto);
        Task<IActionResult> LoginAsync(LoginCompanyDto dto);
        Task<IActionResult> VerifyEmailAsync(VerifyCompanyEmailDto dto);
        Task<IActionResult> UpdateOverviewAsync(UpdateOverviewDto dto);
    }
}
