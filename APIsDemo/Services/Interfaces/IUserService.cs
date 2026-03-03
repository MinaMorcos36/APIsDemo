using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.DTOs.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace APIsDemo.Services.Interfaces
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
        Task<IActionResult> GetCurrentUserAsync();
        Task<IActionResult> SecretAdminAreaAsync();
    }
}
