using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Interfaces;
using Neoron.API.Tests.Fixtures;
using Neoron.API.Models;
using FluentAssertions;

namespace Neoron.API.Tests.Integration
{
    [Collection("Database")]
    public class RepositoryQueryTests : IntegrationTestBase
    {
        private readonly IDiscordMessageRepository _messageRepository;
        private readonly IStagingRepository _stagingRepository;

        public RepositoryQueryTests(TestWebApplicationFactory<Program> factory) 
            : base(factory)
        {
            _messageRepository = factory.Services.GetRequiredService<IDiscordMessageRepository>();
            _stagingRepository = factory.Services.GetRequiredService<IStagingRepository>();
        }

        [Fact]
        public async Task GetByChannelId_ShouldReturnMessagesForChannel()
        {
            // Arrange
            const long channelId = 123;
            var messages = new[]
            {
                new DiscordMessage
                {
                    Id = 1,
                    ChannelId = channelId,
                    GuildId = 456,
                    AuthorId = 789,
                    Content = "Test message 1",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DiscordMessage
                {
                    Id = 2,
                    ChannelId = channelId,
                    GuildId = 456,
                    AuthorId = 789,
                    Content = "Test message 2",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            await _messageRepository.AddRangeAsync(messages);

            // Act
            var result = await _messageRepository.GetByChannelIdAsync(channelId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => m.ChannelId == channelId);
        }

        [Fact]
        public async Task GetByGuildId_ShouldReturnMessagesForGuild()
        {
            // Arrange
            const long guildId = 456;
            var messages = new[]
            {
                new DiscordMessage
                {
                    Id = 3,
                    ChannelId = 123,
                    GuildId = guildId,
                    AuthorId = 789,
                    Content = "Guild message 1",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DiscordMessage
                {
                    Id = 4,
                    ChannelId = 124,
                    GuildId = guildId,
                    AuthorId = 789,
                    Content = "Guild message 2",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            await _messageRepository.AddRangeAsync(messages);

            // Act
            var result = await _messageRepository.GetByGuildIdAsync(guildId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => m.GuildId == guildId);
        }

        [Fact]
        public async Task GetByAuthorId_ShouldReturnMessagesFromAuthor()
        {
            // Arrange
            const long authorId = 789;
            var messages = new[]
            {
                new DiscordMessage
                {
                    Id = 5,
                    ChannelId = 123,
                    GuildId = 456,
                    AuthorId = authorId,
                    Content = "Author message 1",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DiscordMessage
                {
                    Id = 6,
                    ChannelId = 123,
                    GuildId = 456,
                    AuthorId = authorId,
                    Content = "Author message 2",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            await _messageRepository.AddRangeAsync(messages);

            // Act
            var result = await _messageRepository.GetByAuthorIdAsync(authorId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => m.AuthorId == authorId);
        }

        [Fact]
        public async Task GetThreadMessages_ShouldReturnMessagesInThread()
        {
            // Arrange
            const long threadId = 7;
            var threadParent = new DiscordMessage
            {
                Id = threadId,
                ChannelId = 123,
                GuildId = 456,
                AuthorId = 789,
                Content = "Thread parent",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var threadMessages = new[]
            {
                new DiscordMessage
                {
                    Id = 8,
                    ChannelId = 123,
                    GuildId = 456,
                    AuthorId = 789,
                    Content = "Thread reply 1",
                    ThreadId = threadId,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DiscordMessage
                {
                    Id = 9,
                    ChannelId = 123,
                    GuildId = 456,
                    AuthorId = 789,
                    Content = "Thread reply 2",
                    ThreadId = threadId,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            await _messageRepository.AddAsync(threadParent);
            await _messageRepository.AddRangeAsync(threadMessages);

            // Act
            var result = await _messageRepository.GetThreadMessagesAsync(threadId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => m.ThreadId == threadId);
        }

        [Fact]
        public async Task GetByGroupId_ShouldReturnMessagesInGroup()
        {
            // Arrange
            const long groupId = 1;
            var group = new ChannelGroup
            {
                Id = groupId,
                GuildId = 456,
                Name = "Test Group",
                CreatedAt = DateTimeOffset.UtcNow,
                LastActiveAt = DateTimeOffset.UtcNow
            };

            var messages = new[]
            {
                new DiscordMessage
                {
                    Id = 10,
                    ChannelId = 123,
                    GuildId = 456,
                    AuthorId = 789,
                    GroupId = groupId,
                    Content = "Group message 1",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DiscordMessage
                {
                    Id = 11,
                    ChannelId = 123,
                    GuildId = 456,
                    AuthorId = 789,
                    GroupId = groupId,
                    Content = "Group message 2",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            await _messageRepository.CreateChannelGroupAsync(group);
            await _messageRepository.AddRangeAsync(messages);

            // Act
            var result = await _messageRepository.GetByGroupIdAsync(groupId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => m.GroupId == groupId);
        }

        [Fact]
        public async Task GetPendingMessages_ShouldReturnPendingStagedMessages()
        {
            // Arrange
            var messages = new[]
            {
                new DiscordMessage
                {
                    Id = 12,
                    ChannelId = 123,
                    GuildId = 456,
                    AuthorId = 789,
                    Content = "Staged message 1",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DiscordMessage
                {
                    Id = 13,
                    ChannelId = 123,
                    GuildId = 456,
                    AuthorId = 789,
                    Content = "Staged message 2",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            await _stagingRepository.StageMessagesAsync(messages);

            // Act
            var result = await _stagingRepository.GetPendingMessagesAsync(10);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => m.Status == StagingStatus.Pending);
        }

        [Fact]
        public async Task GetPendingAttachments_ShouldReturnPendingStagedAttachments()
        {
            // Arrange
            const long messageId = 14;
            var message = new DiscordMessage
            {
                Id = messageId,
                ChannelId = 123,
                GuildId = 456,
                AuthorId = 789,
                Content = "Message with attachments",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var attachments = new[]
            {
                new DiscordFileAttachment
                {
                    MessageId = messageId,
                    FileName = "test1.txt",
                    FileSize = 100,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DiscordFileAttachment
                {
                    MessageId = messageId,
                    FileName = "test2.txt",
                    FileSize = 200,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            await _messageRepository.AddAsync(message);
            await _stagingRepository.StageAttachmentsAsync(messageId, attachments);

            // Act
            var result = await _stagingRepository.GetPendingAttachmentsAsync(10);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(a => a.Status == AttachmentStatus.Pending);
        }

        [Fact]
        public async Task GetChannelGroups_ShouldReturnGroupsForGuild()
        {
            // Arrange
            const long guildId = 456;
            var groups = new[]
            {
                new ChannelGroup
                {
                    GuildId = guildId,
                    Name = "Group 1",
                    CreatedAt = DateTimeOffset.UtcNow,
                    LastActiveAt = DateTimeOffset.UtcNow
                },
                new ChannelGroup
                {
                    GuildId = guildId,
                    Name = "Group 2",
                    CreatedAt = DateTimeOffset.UtcNow,
                    LastActiveAt = DateTimeOffset.UtcNow
                }
            };

            foreach (var group in groups)
            {
                await _messageRepository.CreateChannelGroupAsync(group);
            }

            // Act
            var result = await _messageRepository.GetChannelGroupsAsync(guildId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(g => g.GuildId == guildId);
        }
    }
}
