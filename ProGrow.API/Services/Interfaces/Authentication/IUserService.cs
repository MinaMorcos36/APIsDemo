using ProGrow.API.DTOs.Auth.JobSeeker;
using ProGrow.API.DTOs.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace ProGrow.API.Services.Interfaces.Authentication
{
    public interface IUserService
    {
        Task<IActionResult> RegisterAsync(RegisterUserDto dto);
        Task<IActionResult> LoginAsync(LoginUserDto dto);
        Task<IActionResult> GoogleCallbackAsync();
        Task<IActionResult> VerifyEmailAsync(VerifyUserEmailDto dto);
        Task<IActionResult> UpdateProfileAsync(UpdateProfileDto dto);
        Task<IActionResult> GetProfileAsync();
        Task<IActionResult> GetSavedPostsAsync();
        Task<IActionResult> SearchSkillsAsync(string query);
        Task<IActionResult> AddSkillAsync(AddUserSkillDto dto);
        Task<IActionResult> GetUserSkillsAsync();
        Task<IActionResult> RemoveSkillAsync(int skillId);
    }
}
