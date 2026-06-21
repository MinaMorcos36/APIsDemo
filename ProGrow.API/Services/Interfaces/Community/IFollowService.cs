using ProGrow.API.DTOs.Community.Follows;

namespace ProGrow.API.Services.Interfaces.Community
{
    public interface IFollowService
    {
        // Toggle-style methods similar to Save services
        Task<bool> ToggleUserFollowAsync(int targetUserId);
        Task<bool> ToggleCompanyFollowAsync(int targetCompanyId);
        Task<bool> ToggleCompanyFollowUserAsync(int targetUserId);
        Task<bool> ToggleCompanyFollowCompanyAsync(int targetCompanyId);
        Task<ProfileCountsDto> GetUserProfileCountsAsync(int userId);
        Task<ProfileCountsDto> GetCompanyOverviewCountsAsync(int companyId);
    }
}
