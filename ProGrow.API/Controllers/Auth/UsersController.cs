using ProGrow.API.DTOs.Auth.JobSeeker;
using ProGrow.API.DTOs.UserProfile;
using ProGrow.API.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProGrow.API.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        #region Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            return await _userService.RegisterAsync(dto);
        }
        #endregion

        #region Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            return await _userService.LoginAsync(dto);
        }
        #endregion

        #region GoogleLogin
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback));
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme); // "Google"
        }
        #endregion

        #region GoogleCallback
        // 2) Google will redirect here after user signs-in/consents
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            return await _userService.GoogleCallbackAsync();
        }
        #endregion

        #region VerifyEmail
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyUserEmailDto dto)
        {
            return await _userService.VerifyEmailAsync(dto);
        }
        #endregion

        #region UpdateProfile
        [Authorize(Policy = "JobSeekerOnly")]
        [HttpPatch("me/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            return await _userService.UpdateProfileAsync(dto);
        }

        #endregion

        #region UploadProfilePhoto
        [Authorize(Policy = "JobSeekerOnly")]
        [HttpPost("me/profile/photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile photo)
        {
            return await _userService.UploadProfilePhotoAsync(photo);
        }
        #endregion

        #region UploadProfileCV
        [Authorize(Policy = "JobSeekerOnly")]
        [HttpPost("me/profile/cv")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCv(IFormFile cv)
        {
            return await _userService.UploadCvAsync(cv);
        }

        #endregion

        #region GetUserProfile
        [Authorize(Policy = "JobSeekerOnly")]
        [HttpGet("me/profile")]
        public async Task<IActionResult> GetProfile()
        {
            return await _userService.GetProfileAsync();
        }
        #endregion

        #region GetProfilePhoto
        [AllowAnonymous]
        [HttpGet("{userId}/photo")]
        public async Task<IActionResult> GetProfilePhoto([FromRoute] int userId)
        {
            return await _userService.GetUserPhotoAsync(userId);
        }
        #endregion

        #region Saved Posts
        [Authorize(Policy = "JobSeekerOnly")]
        [HttpGet("SavedPosts")]
        public async Task<IActionResult> GetSavedPosts()
        {
            return await _userService.GetSavedPostsAsync();
        }
        #endregion

        #region Skills
        [Authorize]
        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            return await _userService.GetSkillsAsync();
        }

        [Authorize(Policy = "JobSeekerOnly")]
        [HttpPost("skills")]
        public async Task<IActionResult> AddSkill([FromBody] AddUserSkillDto dto)
        {
            return await _userService.AddSkillAsync(dto);
        }

        [Authorize(Policy = "JobSeekerOnly")]
        [HttpGet("me/skills")]
        public async Task<IActionResult> GetUserSkills()
        {
            return await _userService.GetUserSkillsAsync();
        }

        [Authorize(Policy = "JobSeekerOnly")]
        [HttpDelete("skills/{id}")]
        public async Task<IActionResult> RemoveSkill([FromRoute] int id)
        {
            return await _userService.RemoveSkillAsync(id);
        }
        #endregion

        #region Logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            return await _userService.LogoutAsync();
        }
        #endregion
    }
}
