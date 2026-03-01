using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.DTOs.UserProfile;

namespace APIsDemo.Services.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(RegisterUserDto dto);

        Task<(string Token, List<string> Roles)> LoginAsync(LoginUserDto dto);

        Task VerifyEmailAsync(VerifyUserEmailDto dto);

        Task UpdateProfileAsync(int userId, UpdateProfileDto dto);

        Task<ProfileResponseDto> GetProfileAsync(int userId);
    }
}
