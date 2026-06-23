using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatService.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace ChatService.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration config, ILogger<TokenService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("UserId", user.Id)
                };

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogDebug("Token generated for user: {Username}", user.Username);

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token for user: {Username}", user.Username);
                throw;
            }
        }
    }
}
