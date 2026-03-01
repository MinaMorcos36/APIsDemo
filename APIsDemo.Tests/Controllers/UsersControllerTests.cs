using APIsDemo.Controllers.Auth;
using APIsDemo.DTOs.Auth.JobSeeker;
using APIsDemo.Models;
using APIsDemo.Services.Implementations;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace APIsDemo.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UsersController _controller;
        private readonly JwtService _jwtService;

        public UsersControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _mockEmailService = new Mock<IEmailService>();

            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x["Issuer"]).Returns("testIssuer");
            mockConfigSection.Setup(x => x["Audience"]).Returns("testAudience");
            mockConfigSection.Setup(x => x["Key"]).Returns("ThisIsAVeryLongSuperSecretKeyForTesting123!");
            mockConfigSection.Setup(x => x["ExpiresInMinutes"]).Returns("60");
            mockConfig.Setup(x => x.GetSection("Jwt")).Returns(mockConfigSection.Object);
            mockConfig.Setup(x => x.GetSection("JWT")).Returns(mockConfigSection.Object);

            _jwtService = new JwtService(mockConfig.Object);

            var userService = new UserService(_context, _jwtService, _mockEmailService.Object);
            _controller = new UsersController(_context, _jwtService, _mockEmailService.Object, userService);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOk()
        {
            // Arrange
            var dto = new RegisterUserDto
            {
                Email = "test@test.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Birthdate = new DateOnly(1990, 1, 1),
                Phone = "1234567890"
            };

            _mockEmailService.Setup(x => x.GenerateOtp(6)).Returns("123456");

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User registered! OTP sent to email.", okResult.Value);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            Assert.NotNull(user);
            Assert.False(user.IsVerified);
            Assert.Equal("123456", user.Otp);
        }

        [Fact]
        public async Task Login_UnverifiedUser_ReturnsUnauthorized()
        {
            // Arrange
            var user = new User
            {
                Email = "unverified@test.com",
                IsVerified = false,
                PasswordHash = "hashedpwd"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new LoginUserDto
            {
                Email = "unverified@test.com",
                Password = "any"
            };

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Email not verified. Please verify your email before login.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task VerifyEmail_ValidOtp_ReturnsOk()
        {
            // Arrange
            var user = new User
            {
                Email = "verify@test.com",
                IsVerified = false,
                Otp = "123456",
                Otpexpiry = DateTime.UtcNow.AddMinutes(10),
                PasswordHash = "fakehash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new VerifyUserEmailDto
            {
                Email = "verify@test.com",
                Otp = "123456"
            };

            // Act
            var result = await _controller.VerifyEmail(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Email verified successfully!", okResult.Value);

            var verifiedUser = await _context.Users.FirstAsync(u => u.Email == dto.Email);
            Assert.True(verifiedUser.IsVerified);
            Assert.Null(verifiedUser.Otp);
            Assert.Null(verifiedUser.Otpexpiry);
        }

        [Fact]
        public async Task Login_ValidVerifiedUser_ReturnsToken()
        {
            // Arrange
            var user = new User
            {
                Email = "valid@test.com",
                IsVerified = true,
                IsActive = true
            };
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, "Password123!");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new LoginUserDto
            {
                Email = "valid@test.com",
                Password = "Password123!"
            };

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Check if anonymous type contains "Token"
            var value = okResult.Value;
            var tokenProperty = value!.GetType().GetProperty("Token");
            Assert.NotNull(tokenProperty);

            var tokenValue = tokenProperty.GetValue(value) as string;
            Assert.False(string.IsNullOrEmpty(tokenValue));
        }
    }
}
