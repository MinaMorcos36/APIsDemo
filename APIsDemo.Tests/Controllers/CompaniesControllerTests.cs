using APIsDemo.Controllers.Auth;
using APIsDemo.DTOs.Auth.Company;
using APIsDemo.Models;
using APIsDemo.Services.Implementations;
using APIsDemo.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace APIsDemo.Tests.Controllers
{
    public class CompaniesControllerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly CompaniesController _controller;
        private readonly JwtService _jwtService;

        public CompaniesControllerTests()
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

            var companyService = new CompanyService(_context, _jwtService, _mockEmailService.Object);
            _controller = new CompaniesController(_context, _jwtService, _mockEmailService.Object, companyService);
        }

        [Fact]
        public async Task Register_ValidCompany_ReturnsOk()
        {
            // Arrange
            var dto = new RegisterCompanyDto
            {
                Name = "Tech Corp",
                Email = "hr@techcorp.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _mockEmailService.Setup(x => x.GenerateOtp(6)).Returns("654321");

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Company registered! OTP sent to email.", okResult.Value);

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == dto.Email);
            Assert.NotNull(company);
            Assert.False(company.IsVerified);
            Assert.Equal("654321", company.Otp);
        }

        [Fact]
        public async Task Login_ValidVerifiedCompany_ReturnsToken()
        {
            // Arrange
            var company = new Company
            {
                Email = "valid@testco.com",
                IsVerified = true,
                IsActive = true
            };
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Company>();
            company.PasswordHash = hasher.HashPassword(company, "Password123!");

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var dto = new LoginCompanyDto
            {
                Email = "valid@testco.com",
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
