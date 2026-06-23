using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ChatService.Api.Models;
using ChatService.Api.Services;


namespace ChatService.Api.WebSockets
{
    public class ChatWebSocketHandler
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly IChatMessageService _messageService;
        private readonly ILogger<ChatWebSocketHandler> _logger;

        public ChatWebSocketHandler(IChatMessageService messageService, ILogger<ChatWebSocketHandler> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        public async Task HandleAsync(WebSocket socket, string username)
        {
            // Handle duplicate sessions
            if (_connections.TryGetValue(username, out var existingSocket))
            {
                await CloseSocketIfOpenAsync(existingSocket, "Duplicate session");
                _connections.TryRemove(username, out _);
            }

            _connections.TryAdd(username, socket);
            _logger.LogInformation("User connected: {Username}", username);

            try
            {
                // Send chat history
                var history = await _messageService.GetRecentMessagesAsync(50);
                var normalizedHistory = history.Select(m => new
                {
                    type = "message",
                    username = m.Username,
                    text = m.Text,
                    timestamp = m.Timestamp
                });
                var historyJson = JsonSerializer.Serialize(new { type = "history", messages = normalizedHistory });
                await SendMessageAsync(socket, historyJson);

                // Notify others about new user
                await BroadcastAsync(new { type = "user_joined", username });

                // Process incoming messages
                var buffer = new byte[1024 * 4];
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    if (!result.EndOfMessage)
                    {
                        continue;
                    }

                    var text = Encoding.UTF8.GetString(buffer, 0, result.Count).Trim();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    string? messageText;
                    try
                    {
                        var messageObj = JsonSerializer.Deserialize<JsonElement>(text);
                        if (!messageObj.TryGetProperty("text", out var textProperty))
                        {
                            continue;
                        }

                        messageText = textProperty.GetString()?.Trim();
                        if (string.IsNullOrWhiteSpace(messageText))
                        {
                            continue;
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Invalid JSON received from user: {Username}", username);
                        continue;
                    }

                    // Save message
                    var message = new ChatMessage
                    {
                        Username = username,
                        Text = messageText,
                        Timestamp = DateTime.UtcNow
                    };

                    try
                    {
                        await _messageService.SaveMessageAsync(message);
                        await BroadcastAsync(new
                        {
                            type = "message",
                            username = message.Username,
                            text = message.Text,
                            timestamp = message.Timestamp
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving message from user: {Username}", username);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling WebSocket for user: {Username}", username);
            }
            finally
            {
                _connections.TryRemove(username, out _);
                await CloseSocketIfOpenAsync(socket, "Connection closed");
                await BroadcastAsync(new { type = "user_left", username });
                _logger.LogInformation("User disconnected: {Username}", username);
            }
        }

        private async Task BroadcastAsync(object message)
        {
            var json = JsonSerializer.Serialize(message);
            var tasks = _connections.Values
                .Where(s => s.State == WebSocketState.Open)
                .Select(s => SendMessageAsync(s, json));

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting message");
            }
        }

        private async Task SendMessageAsync(WebSocket socket, string message)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to WebSocket");
            }
        }

        private static async Task CloseSocketIfOpenAsync(WebSocket socket, string reason)
        {
            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        reason,
                        CancellationToken.None);
                }
            }
            catch (Exception)
            {
                // Socket might already be closed
            }
        }
    }
}