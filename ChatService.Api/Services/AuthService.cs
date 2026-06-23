using ChatService.Api.Models;
using MongoDB.Driver;

namespace ChatService.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _users;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IMongoCollection<User> users, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _users = users;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string? Token)> RegisterAsync(string username, string password)
        {
            try
            {
                var exists = await _users.Find(u => u.Username == username).AnyAsync();
                if (exists)
                {
                    _logger.LogWarning("Registration attempt with existing username: {Username}", username);
                    return (false, "Username is already taken", null);
                }

                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    CreatedAt = DateTime.UtcNow
                };

                await _users.InsertOneAsync(user);
                var token = _tokenService.GenerateToken(user);

                _logger.LogInformation("User registered successfully: {Username}", username);
                return (true, "Registration successful", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {Username}", username);
                throw;
            }
        }

        public async Task<(bool Success, string Message, string? Token)> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent username: {Username}", username);
                    return (false, "Invalid username or password", null);
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for username: {Username}", username);
                    return (false, "Invalid username or password", null);
                }

                var token = _tokenService.GenerateToken(user);
                _logger.LogInformation("User logged in successfully: {Username}", username);

                return (true, "Login successful", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", username);
                throw;
            }
        }
    }
}
