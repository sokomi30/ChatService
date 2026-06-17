using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ChatService.Api.Models;
using MongoDB.Driver;


namespace ChatService.Api.WebSockets
{
    public class ChatWebSocketHandler
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly IMongoCollection<ChatMessage> _messages;

        public ChatWebSocketHandler(IMongoCollection<ChatMessage> messages)
        {
            _messages = messages;
        }

        public async Task HandleAsync(WebSocket socket, string username)
        {
            _connections.TryAdd(username, socket);

            // Отправляем историю
            var history = await _messages.Find(_ => true).SortByDescending(m => m.Timestamp).Limit(50).ToListAsync();
            history.Reverse();
            var historyJson = JsonSerializer.Serialize(new { type = "history", messages = history });
            await SendMessageAsync(socket, historyJson);

            // Уведомляем всех о новом пользователе
            await BroadcastAsync(new { type = "user_joined", username });

            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var messageObj = JsonSerializer.Deserialize<JsonElement>(text);
                var messageText = messageObj.GetProperty("text").GetString();

                var message = new ChatMessage
                {
                    Username = username,
                    Text = messageText!,
                    Timestamp = DateTime.UtcNow
                };

                await _messages.InsertOneAsync(message);
                await BroadcastAsync(new { type = "message", username = message.Username, text = message.Text, timestamp = message.Timestamp });
            }

            _connections.TryRemove(username, out _);
            await BroadcastAsync(new { type = "user_left", username });
        }

        private async Task BroadcastAsync(object message)
        {
            var json = JsonSerializer.Serialize(message);
            var tasks = _connections.Values.Where(s => s.State == WebSocketState.Open)
                .Select(s => SendMessageAsync(s, json));
            await Task.WhenAll(tasks);
        }

        private async Task SendMessageAsync(WebSocket socket, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}