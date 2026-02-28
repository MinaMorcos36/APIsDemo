using APIsDemo.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace APIsDemo.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;
        private readonly Mock<IConfiguration> _mockConfig;

        public JwtServiceTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x["Issuer"]).Returns("testIssuer");
            mockConfigSection.Setup(x => x["Audience"]).Returns("testAudience");
            mockConfigSection.Setup(x => x["Key"]).Returns("ThisIsAVeryLongSuperSecretKeyForTesting123!");
            mockConfigSection.Setup(x => x["ExpireDays"]).Returns("7");

            mockConfigSection.Setup(x => x["ExpiresInMinutes"]).Returns("60");
            _mockConfig.Setup(x => x.GetSection("Jwt")).Returns(mockConfigSection.Object);
            _mockConfig.Setup(x => x.GetSection("JWT")).Returns(mockConfigSection.Object);

            _jwtService = new JwtService(_mockConfig.Object);
        }

        [Fact]
        public void GenerateToken_ReturnsValidJwtToken()
        {
            // Arrange
            var userId = 1;
            var authorType = "JobSeeker";
            var email = "test@test.com";
            var roles = new List<string> { "User", "Admin" };

            // Act
            var tokenString = _jwtService.GenerateToken(userId, authorType, email, roles);

            // Assert
            Assert.False(string.IsNullOrEmpty(tokenString));

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenString);

            Assert.Equal("testIssuer", jwtToken.Issuer);
            Assert.Equal("testAudience", jwtToken.Audiences.First());

            var nameIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            Assert.NotNull(nameIdClaim);
            Assert.Equal("1", nameIdClaim.Value);

            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
            Assert.NotNull(emailClaim);
            Assert.Equal(email, emailClaim.Value);

            var authorTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "AuthorType");
            Assert.NotNull(authorTypeClaim);
            Assert.Equal(authorType, authorTypeClaim.Value);

            var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.Contains("User", roleClaims);
            Assert.Contains("Admin", roleClaims);
        }
    }
}
