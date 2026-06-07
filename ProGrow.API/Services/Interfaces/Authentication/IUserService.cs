using ProGrow.API.DTOs.Auth.JobSeeker;
using ProGrow.API.DTOs.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ProGrow.API.Services.Interfaces.Authentication
{
    public interface IUserService
    {
        Task<IActionResult> RegisterAsync(RegisterUserDto dto);
        Task<IActionResult> LoginAsync(LoginUserDto dto);
        Task<IActionResult> GoogleCallbackAsync();
        Task<IActionResult> VerifyEmailAsync(VerifyUserEmailDto dto);
        Task<IActionResult> UpdateProfileAsync(UpdateProfileDto dto);
        Task<IActionResult> UploadProfilePhotoAsync(IFormFile photo);
        Task<IActionResult> UploadCvAsync(IFormFile cv);
        Task<IActionResult> GetUserPhotoAsync(int userId);
        Task<IActionResult> GetProfileAsync();
        Task<IActionResult> GetSavedItemsAsync();
        Task<IActionResult> GetSkillsAsync();
        Task<IActionResult> AddSkillAsync(AddUserSkillDto dto);
        Task<IActionResult> GetUserSkillsAsync();
        Task<IActionResult> RemoveSkillAsync(int skillId);
        Task<IActionResult> LogoutAsync();
    }
}
