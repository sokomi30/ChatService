using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using ChatService.Api.Models;
using ChatService.Api.Services;
using MongoDB.Driver;

namespace ChatService.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IMongoCollection<User>> _mockUserCollection;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserCollection = new Mock<IMongoCollection<User>>();
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _authService = new AuthService(_mockUserCollection.Object, _mockTokenService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithNewUser_ReturnsSuccessAndToken()
        {
            // Arrange
            var username = "newuser";
            var password = "password123";
            var token = "test-token-123";

            var mockFindFluent = new Mock<IFindFluent<User, User>>();
            mockFindFluent
                .Setup(f => f.AnyAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockUserCollection
                .Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(mockFindFluent.Object);

            _mockTokenService
                .Setup(t => t.GenerateToken(It.IsAny<User>()))
                .Returns(token);

            _mockUserCollection
                .Setup(c => c.InsertOneAsync(It.IsAny<User>(), null, CancellationToken.None))
                .Returns(Task.CompletedTask);

            // Act
            var (success, message, returnedToken) = await _authService.RegisterAsync(username, password);

            // Assert
            Assert.True(success);
            Assert.Equal(token, returnedToken);
            _mockUserCollection.Verify(c => c.InsertOneAsync(It.IsAny<User>(), null, CancellationToken.None), Times.Once);
            _mockTokenService.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ReturnsFailure()
        {
            // Arrange
            var username = "existinguser";
            var password = "password123";

            var mockFindFluent = new Mock<IFindFluent<User, User>>();
            mockFindFluent
                .Setup(f => f.AnyAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockUserCollection
                .Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(mockFindFluent.Object);

            // Act
            var (success, message, token) = await _authService.RegisterAsync(username, password);

            // Assert
            Assert.False(success);
            Assert.Null(token);
            Assert.Contains("already taken", message);
            _mockUserCollection.Verify(c => c.InsertOneAsync(It.IsAny<User>(), null, CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccessAndToken()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var token = "test-token-456";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Id = "user-id-123",
                Username = username,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            var mockFindFluent = new Mock<IFindFluent<User, User>>();
            mockFindFluent
                .Setup(f => f.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserCollection
                .Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(mockFindFluent.Object);

            _mockTokenService
                .Setup(t => t.GenerateToken(It.IsAny<User>()))
                .Returns(token);

            // Act
            var (success, message, returnedToken) = await _authService.LoginAsync(username, password);

            // Assert
            Assert.True(success);
            Assert.Equal(token, returnedToken);
            _mockTokenService.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsFailure()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpassword";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");

            var user = new User
            {
                Id = "user-id-123",
                Username = username,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            var mockFindFluent = new Mock<IFindFluent<User, User>>();
            mockFindFluent
                .Setup(f => f.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserCollection
                .Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(mockFindFluent.Object);

            // Act
            var (success, message, token) = await _authService.LoginAsync(username, password);

            // Assert
            Assert.False(success);
            Assert.Null(token);
            Assert.Contains("Invalid", message);
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            var username = "nonexistent";
            var password = "password123";

            var mockFindFluent = new Mock<IFindFluent<User, User>>();
            mockFindFluent
                .Setup(f => f.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockUserCollection
                .Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(mockFindFluent.Object);

            // Act
            var (success, message, token) = await _authService.LoginAsync(username, password);

            // Assert
            Assert.False(success);
            Assert.Null(token);
            Assert.Contains("Invalid", message);
        }
    }
}
