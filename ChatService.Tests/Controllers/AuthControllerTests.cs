using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChatService.Api.Controllers;
using ChatService.Api.Models;
using ChatService.Api.Services;

namespace ChatService.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Register_WithValidDto_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new LoginDto { Username = "newuser", Password = "password123" };
            var token = "test-jwt-token";

            _mockAuthService
                .Setup(s => s.RegisterAsync(dto.Username, dto.Password))
                .ReturnsAsync((true, "Registration successful", token));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var value = okResult.Value as dynamic;
            Assert.Equal(token, value?.token);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ReturnsConflict()
        {
            // Arrange
            var dto = new LoginDto { Username = "existinguser", Password = "password123" };

            _mockAuthService
                .Setup(s => s.RegisterAsync(dto.Username, dto.Password))
                .ReturnsAsync((false, "Username is already taken", (string?)null));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);

            var errorResponse = conflictResult.Value as ErrorResponse;
            Assert.NotNull(errorResponse);
            Assert.Contains("already taken", errorResponse.Message);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new LoginDto { Username = "testuser", Password = "password123" };
            var token = "test-jwt-token";

            _mockAuthService
                .Setup(s => s.LoginAsync(dto.Username, dto.Password))
                .ReturnsAsync((true, "Login successful", token));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var value = okResult.Value as dynamic;
            Assert.Equal(token, value?.token);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginDto { Username = "testuser", Password = "wrongpassword" };

            _mockAuthService
                .Setup(s => s.LoginAsync(dto.Username, dto.Password))
                .ReturnsAsync((false, "Invalid username or password", (string?)null));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);

            var errorResponse = unauthorizedResult.Value as ErrorResponse;
            Assert.NotNull(errorResponse);
            Assert.Contains("Invalid", errorResponse.Message);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginDto { Username = "nonexistent", Password = "password123" };

            _mockAuthService
                .Setup(s => s.LoginAsync(dto.Username, dto.Password))
                .ReturnsAsync((false, "Invalid username or password", (string?)null));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }
    }
}
