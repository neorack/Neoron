using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Interfaces;
using Neoron.API.Tests.Fixtures;
using Neoron.API.Models;
using FluentAssertions;

namespace Neoron.API.Tests.Integration
{
    [Collection("Database")]
    public class SyncEdgeCaseTests : IntegrationTestBase
    {
        private readonly IDiscordLogIngestionService _ingestionService;
        private readonly IDiscordMessageRepository _messageRepository;

        public SyncEdgeCaseTests(TestWebApplicationFactory<Program> factory) 
            : base(factory)
        {
            _ingestionService = factory.Services.GetRequiredService<IDiscordLogIngestionService>();
            _messageRepository = factory.Services.GetRequiredService<IDiscordMessageRepository>();
        }

        [Fact]
        public async Task SyncMessages_WithDeletedMessages_ShouldHandleGracefully()
        {
            // Arrange
            const long guildId = 123;
            const long channelId = 456;
            var message = new DiscordMessage
            {
                Id = 789,
                GuildId = guildId,
                ChannelId = channelId,
                Content = "Test message",
                CreatedAt = DateTimeOffset.UtcNow,
                IsDeleted = true
            };

            await _messageRepository.AddAsync(message);

            // Act
            var syncedCount = await _ingestionService.SyncMessagesAsync(guildId, channelId);

            // Assert
            syncedCount.Should().Be(0); // Should not count deleted messages
        }

        [Fact]
        public async Task SyncMessages_WithConcurrentSync_ShouldHandleLocking()
        {
            // Arrange
            const long guildId = 234;
            const long channelId = 567;
            
            var message = new DiscordMessage
            {
                Id = 890,
                GuildId = guildId,
                ChannelId = channelId,
                Content = "Test message",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _messageRepository.AddAsync(message);

            // Act
            var task1 = _ingestionService.SyncMessagesAsync(guildId, channelId);
            var task2 = _ingestionService.SyncMessagesAsync(guildId, channelId);

            // Act
            var tasks = new List<Task<int>>
            {
                task1,
                task2,
                _ingestionService.SyncMessagesAsync(guildId, channelId), // Add third concurrent sync
                _ingestionService.SyncMessagesAsync(guildId, channelId)  // Add fourth concurrent sync
            };

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Sum().Should().Be(1); // Message should only be counted once
            var checkpoint = await _ingestionService.GetSyncCheckpointAsync(guildId, channelId);
            checkpoint.Should().NotBeNull();
            checkpoint!.LastMessageId.Should().Be(message.Id);
        }

        [Fact]
        public async Task SyncMessages_WithLargeGap_ShouldHandlePagination()
        {
            // Arrange
            const long guildId = 345;
            const long channelId = 678;
            
            var messages = Enumerable.Range(0, 1500).Select(i => new DiscordMessage
            {
                Id = 1000 + i,
                GuildId = guildId,
                ChannelId = channelId,
                Content = $"Message {i}",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-i)
            }).ToArray();

            await _messageRepository.AddRangeAsync(messages);

            // Act
            var syncedCount = await _ingestionService.SyncMessagesAsync(guildId, channelId);

            // Assert
            syncedCount.Should().Be(1500);
            var checkpoint = await _ingestionService.GetSyncCheckpointAsync(guildId, channelId);
            checkpoint.Should().NotBeNull();
            checkpoint!.LastMessageId.Should().Be(messages.Max(m => m.Id));
        }
    }
}
