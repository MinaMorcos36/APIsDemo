using APIsDemo.DTOs.Auth.Company;
using APIsDemo.DTOs.CompanyOverview;

namespace APIsDemo.Services.Interfaces
{
    public interface ICompanyService
    {
        Task RegisterAsync(RegisterCompanyDto dto);

        Task<string> LoginAsync(LoginCompanyDto dto);

        Task VerifyEmailAsync(VerifyCompanyEmailDto dto);

        Task UpdateOverviewAsync(int companyId, UpdateOverviewDto dto);
    }
}
