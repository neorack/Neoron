using Neoron.API.IntegrationTests.Fixtures;
using Neoron.API.Models;
using Neoron.API.Repositories;
using Xunit;

namespace Neoron.API.IntegrationTests.Repositories;

public class DiscordMessageRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly DiscordMessageRepository _repository;

    public DiscordMessageRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new DiscordMessageRepository(_fixture.DbContext);
    }

    [Fact]
    public async Task AddAsync_WithRelatedMessages_ShouldPersistRelationships()
    {
        // Arrange
        var parentMessage = new DiscordMessage
        {
            MessageId = 1,
            Content = "Parent message",
            ChannelId = 100,
            GuildId = 200,
            AuthorId = 300,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var replyMessage = new DiscordMessage
        {
            MessageId = 2,
            Content = "Reply message",
            ChannelId = 100,
            GuildId = 200,
            AuthorId = 301,
            ReplyToMessageId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        await _repository.AddAsync(parentMessage);
        await _repository.AddAsync(replyMessage);

        // Assert
        var retrievedReply = await _repository.GetByIdAsync(2L);
        Assert.NotNull(retrievedReply);
        Assert.Equal(1L, retrievedReply.ReplyToMessageId);
    }

    [Fact]
    public async Task GetByChannelIdAsync_WithMultipleMessages_ShouldReturnCorrectOrder()
    {
        // Arrange
        var messages = new List<DiscordMessage>
        {
            new()
            {
                MessageId = 3,
                ChannelId = 101,
                Content = "First message",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2),
                GuildId = 200,
                AuthorId = 300
            },
            new()
            {
                MessageId = 4,
                ChannelId = 101,
                Content = "Second message",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
                GuildId = 200,
                AuthorId = 300
            }
        };

        foreach (var message in messages)
        {
            await _repository.AddAsync(message);
        }

        // Act
        var result = (await _repository.GetByChannelIdAsync(101)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("First message", result[0].Content);
        Assert.Equal("Second message", result[1].Content);
    }

    [Fact]
    public async Task DeleteAsync_WithThreadParent_ShouldNotDeleteThreadMessages()
    {
        // Arrange
        var threadParent = new DiscordMessage
        {
            MessageId = 5,
            Content = "Thread parent",
            ChannelId = 102,
            GuildId = 200,
            AuthorId = 300,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var threadMessage = new DiscordMessage
        {
            MessageId = 6,
            Content = "Thread message",
            ChannelId = 102,
            GuildId = 200,
            AuthorId = 301,
            ThreadId = 5,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.AddAsync(threadParent);
        await _repository.AddAsync(threadMessage);

        // Act
        await _repository.DeleteAsync(5L);

        // Assert
        var deletedParent = await _repository.GetByIdAsync(5L);
        var threadChild = await _repository.GetByIdAsync(6L);
        
        Assert.True(deletedParent?.IsDeleted);
        Assert.False(threadChild?.IsDeleted);
    }
}
