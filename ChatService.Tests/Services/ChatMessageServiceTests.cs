using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using ChatService.Api.Models;
using ChatService.Api.Services;
using MongoDB.Driver;

namespace ChatService.Tests.Services
{
    public class ChatMessageServiceTests
    {
        private readonly Mock<IMongoCollection<ChatMessage>> _mockMessageCollection;
        private readonly Mock<ILogger<ChatMessageService>> _mockLogger;
        private readonly ChatMessageService _messageService;

        public ChatMessageServiceTests()
        {
            _mockMessageCollection = new Mock<IMongoCollection<ChatMessage>>();
            _mockLogger = new Mock<ILogger<ChatMessageService>>();
            _messageService = new ChatMessageService(_mockMessageCollection.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetRecentMessagesAsync_ReturnsMessagesInChronologicalOrder()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Id = "1", Username = "user1", Text = "Hello", Timestamp = DateTime.UtcNow.AddMinutes(-2) },
                new ChatMessage { Id = "2", Username = "user2", Text = "Hi", Timestamp = DateTime.UtcNow.AddMinutes(-1) },
                new ChatMessage { Id = "3", Username = "user1", Text = "How are you?", Timestamp = DateTime.UtcNow }
            };

            var mockAsyncCursor = new Mock<IAsyncCursor<ChatMessage>>();
            mockAsyncCursor
                .Setup(c => c.ToListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages.OrderByDescending(m => m.Timestamp).ToList());

            _mockMessageCollection
                .Setup(c => c.Find(It.IsAny<FilterDefinition<ChatMessage>>(), null))
                .Returns(mockAsyncCursor.Object);

            // Act
            var result = await _messageService.GetRecentMessagesAsync(50);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("user1", result.First().Username);
            Assert.Equal("Hello", result.First().Text);
            Assert.Equal("How are you?", result.Last().Text);
        }

        [Fact]
        public async Task SaveMessageAsync_WithValidMessage_SavesSuccessfully()
        {
            // Arrange
            var message = new ChatMessage
            {
                Username = "testuser",
                Text = "Test message",
                Timestamp = DateTime.UtcNow
            };

            _mockMessageCollection
                .Setup(c => c.InsertOneAsync(It.IsAny<ChatMessage>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _messageService.SaveMessageAsync(message);

            // Assert
            _mockMessageCollection.Verify(
                c => c.InsertOneAsync(It.IsAny<ChatMessage>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveMessageAsync_WithEmptyText_ThrowsArgumentException()
        {
            // Arrange
            var message = new ChatMessage
            {
                Username = "testuser",
                Text = "",
                Timestamp = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SaveMessageAsync(message));
            _mockMessageCollection.Verify(
                c => c.InsertOneAsync(It.IsAny<ChatMessage>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SaveMessageAsync_WithNullUsername_ThrowsArgumentException()
        {
            // Arrange
            var message = new ChatMessage
            {
                Username = null!,
                Text = "Test message",
                Timestamp = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SaveMessageAsync(message));
        }
    }
}
