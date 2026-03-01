using APIsDemo.DTOs;
using APIsDemo.DTOs.Auth.Company;
using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.DTOs.CompanyOverview;
using APIsDemo.DTOs.UserProfile;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace APIsDemo.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        #region Services: DBContext,JWT,EmailService
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly IEmailService _emailService;
        private readonly ICompanyService _companyService;
        private AppDbContext context;
        private JwtService jwtService;
        private IEmailService @object;

        public CompaniesController(AppDbContext context, JwtService jwt, IEmailService emailService, ICompanyService companyService)
        {
            _context = context;
            _jwt = jwt;
            _emailService = emailService;
            _companyService = companyService;
        }

        public CompaniesController(AppDbContext context, JwtService jwtService, IEmailService @object)
        {
            this.context = context;
            this.jwtService = jwtService;
            this.@object = @object;
        }
        #endregion

        #region Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterCompanyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _companyService.RegisterAsync(dto);
                return Ok("Company registered! OTP sent to email.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }
        #endregion

        #region Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginCompanyDto dto)
        {
            try
            {
                var token = await _companyService.LoginAsync(dto);
                return Ok(new { Token = token });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
        #endregion

        #region VerifyEmail
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyCompanyEmailDto dto)
        {
            try
            {
                await _companyService.VerifyEmailAsync(dto);
                return Ok("Email verified successfully!");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateOverview
        [Authorize]
        [HttpPatch("me/overview")]
        public async Task<IActionResult> UpdateOverview([FromBody] UpdateOverviewDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            try
            {
                await _companyService.UpdateOverviewAsync(userId, dto);
                return Ok("Overview updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
