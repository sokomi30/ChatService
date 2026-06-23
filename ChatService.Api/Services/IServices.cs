using ChatService.Api.Models;

namespace ChatService.Api.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string? Token)> RegisterAsync(string username, string password);
        Task<(bool Success, string Message, string? Token)> LoginAsync(string username, string password);
    }

    public interface IChatMessageService
    {
        Task<List<ChatMessage>> GetRecentMessagesAsync(int limit = 50);
        Task SaveMessageAsync(ChatMessage message);
    }

    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
