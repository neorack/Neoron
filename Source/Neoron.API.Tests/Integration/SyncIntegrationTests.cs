using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Interfaces;
using Neoron.API.Tests.Fixtures;
using Neoron.API.Models;
using FluentAssertions;

namespace Neoron.API.Tests.Integration
{
    [Collection("Database")]
    public class SyncIntegrationTests : IntegrationTestBase
    {
        private readonly IDiscordLogIngestionService _ingestionService;
        private readonly IDiscordMessageRepository _messageRepository;

        public SyncIntegrationTests(TestWebApplicationFactory<Program> factory) 
            : base(factory)
        {
            _ingestionService = factory.Services.GetRequiredService<IDiscordLogIngestionService>();
            _messageRepository = factory.Services.GetRequiredService<IDiscordMessageRepository>();
        }

        [Fact]
        public async Task SyncMessages_WithNoExistingCheckpoint_ShouldCreateNewCheckpoint()
        {
            // Arrange
            const long guildId = 123;
            const long channelId = 456;
            const long messageId = 789;

            var message = new DiscordMessage
            {
                Id = messageId,
                GuildId = guildId,
                ChannelId = channelId,
                AuthorId = 101112,
                Content = "Test message",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _messageRepository.AddAsync(message);

            // Act
            var syncedCount = await _ingestionService.SyncMessagesAsync(guildId, channelId);
            var checkpoint = await _ingestionService.GetSyncCheckpointAsync(guildId, channelId);

            // Assert
            syncedCount.Should().Be(1);
            checkpoint.Should().NotBeNull();
            checkpoint!.GuildId.Should().Be(guildId);
            checkpoint.ChannelId.Should().Be(channelId);
            checkpoint.LastMessageId.Should().Be(messageId);
            checkpoint.LastSyncedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task SyncMessages_WithExistingCheckpoint_ShouldOnlySyncNewMessages()
        {
            // Arrange
            const long guildId = 234;
            const long channelId = 567;
            
            var oldMessage = new DiscordMessage
            {
                Id = 890,
                GuildId = guildId,
                ChannelId = channelId,
                AuthorId = 101112,
                Content = "Old message",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
            };

            var newMessage = new DiscordMessage
            {
                Id = 891,
                GuildId = guildId,
                ChannelId = channelId,
                AuthorId = 101112,
                Content = "New message",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _messageRepository.AddAsync(oldMessage);
            
            // First sync to create checkpoint
            await _ingestionService.SyncMessagesAsync(guildId, channelId);
            
            // Add new message after checkpoint
            await _messageRepository.AddAsync(newMessage);

            // Act
            var syncedCount = await _ingestionService.SyncMessagesAsync(guildId, channelId);
            var checkpoint = await _ingestionService.GetSyncCheckpointAsync(guildId, channelId);

            // Assert
            syncedCount.Should().Be(1); // Only the new message
            checkpoint.Should().NotBeNull();
            checkpoint!.LastMessageId.Should().Be(newMessage.Id);
        }

        [Fact]
        public async Task SyncMessages_WithSpecificStartPoint_ShouldSyncFromThatPoint()
        {
            // Arrange
            const long guildId = 345;
            const long channelId = 678;
            const long startMessageId = 901;

            var messages = new[]
            {
                new DiscordMessage
                {
                    Id = startMessageId,
                    GuildId = guildId,
                    ChannelId = channelId,
                    AuthorId = 101112,
                    Content = "Start message",
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2)
                },
                new DiscordMessage
                {
                    Id = startMessageId + 1,
                    GuildId = guildId,
                    ChannelId = channelId,
                    AuthorId = 101112,
                    Content = "Later message",
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
                }
            };

            await _messageRepository.AddRangeAsync(messages);

            // Act
            var syncedCount = await _ingestionService.SyncMessagesAsync(guildId, channelId, startMessageId);
            var checkpoint = await _ingestionService.GetSyncCheckpointAsync(guildId, channelId);

            // Assert
            syncedCount.Should().Be(2); // Both messages from start point
            checkpoint.Should().NotBeNull();
            checkpoint!.LastMessageId.Should().Be(startMessageId + 1);
        }

        [Fact]
        public async Task GetSyncCheckpoint_WhenNoCheckpointExists_ShouldReturnNull()
        {
            // Arrange
            const long guildId = 456;
            const long channelId = 789;

            // Act
            var checkpoint = await _ingestionService.GetSyncCheckpointAsync(guildId, channelId);

            // Assert
            checkpoint.Should().BeNull();
        }
    }
}
