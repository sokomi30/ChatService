using System.IdentityModel.Tokens.Jwt;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using ChatService.Api.Models;
using ChatService.Api.Services;
using Microsoft.Extensions.Configuration;

namespace ChatService.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<TokenService>> _mockLogger;
        private readonly TokenService _tokenService;

        private const string TestSecret = "this-is-a-test-secret-key-that-is-long-enough-for-testing-purposes";
        private const string TestIssuer = "TestIssuer";
        private const string TestAudience = "TestAudience";

        public TokenServiceTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<TokenService>>();

            _mockConfig.Setup(c => c["Jwt:Key"]).Returns(TestSecret);
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns(TestIssuer);
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns(TestAudience);

            _tokenService = new TokenService(_mockConfig.Object, _mockLogger.Object);
        }

        [Fact]
        public void GenerateToken_ReturnsValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = "user-123",
                Username = "testuser",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            // Verify token can be read
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            Assert.Equal(TestIssuer, jwtToken.Issuer);
            Assert.Equal(TestAudience, jwtToken.Audiences.FirstOrDefault());

            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            Assert.NotNull(usernameClaim);
            Assert.Equal("testuser", usernameClaim.Value);
        }

        [Fact]
        public void GenerateToken_TokenHasCorrectExpiry()
        {
            // Arrange
            var user = new User
            {
                Id = "user-123",
                Username = "testuser",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            Assert.NotNull(jwtToken.ValidTo);

            // Token should expire in ~2 hours (allowing 1 minute tolerance)
            var timeUntilExpiry = jwtToken.ValidTo - DateTime.UtcNow;
            Assert.True(timeUntilExpiry.TotalMinutes > 119 && timeUntilExpiry.TotalMinutes < 121);
        }

        [Fact]
        public void GenerateToken_IncludesUserIdClaim()
        {
            // Arrange
            var userId = "specific-user-id";
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var userIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "UserId");
            Assert.NotNull(userIdClaim);
            Assert.Equal(userId, userIdClaim.Value);
        }
    }
}
