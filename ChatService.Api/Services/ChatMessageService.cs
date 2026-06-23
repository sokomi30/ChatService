using ChatService.Api.Models;
using MongoDB.Driver;

namespace ChatService.Api.Services
{
    public class ChatMessageService : IChatMessageService
    {
        private readonly IMongoCollection<ChatMessage> _messages;
        private readonly ILogger<ChatMessageService> _logger;

        public ChatMessageService(IMongoCollection<ChatMessage> messages, ILogger<ChatMessageService> logger)
        {
            _messages = messages;
            _logger = logger;
        }

        public async Task<List<ChatMessage>> GetRecentMessagesAsync(int limit = 50)
        {
            try
            {
                var messages = await _messages
                    .Find(_ => true)
                    .SortByDescending(m => m.Timestamp)
                    .Limit(limit)
                    .ToListAsync();

                messages.Reverse(); // Return in chronological order (oldest first)
                _logger.LogDebug("Retrieved {Count} recent messages", messages.Count);

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent messages");
                throw;
            }
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message.Text))
                {
                    throw new ArgumentException("Message text cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(message.Username))
                {
                    throw new ArgumentException("Username is required");
                }

                message.Timestamp = DateTime.UtcNow;
                await _messages.InsertOneAsync(message);

                _logger.LogDebug("Message saved from user: {Username}", message.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving message from user: {Username}", message.Username);
                throw;
            }
        }
    }
}
