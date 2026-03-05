using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.DTOs.UserProfile;
using APIsDemo.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIsDemo.Controllers.Auth
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

        #region DefaultAuthUser
        [Authorize]
        [HttpGet("default")]
        public async Task<IActionResult> GetCurrentUser()
        {
            return await _userService.GetCurrentUserAsync();
        }
        #endregion

        #region Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> SecretAdminArea()
        {
            return await _userService.SecretAdminAreaAsync();
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
        [Authorize]
        [HttpPatch("me/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            return await _userService.UpdateProfileAsync(dto);
        }

        #endregion

        #region GetProfile
        [Authorize]
        [HttpGet("me/profile")]
        public async Task<IActionResult> GetProfile()
        {
            return await _userService.GetProfileAsync();
        }
        #endregion

        #region Saved Posts
        [Authorize]
        [HttpGet("SavedPosts")]
        public async Task<IActionResult> GetSavedPosts()
        {
            return await _userService.GetSavedPostsAsync();
        }
        #endregion
    }
}
