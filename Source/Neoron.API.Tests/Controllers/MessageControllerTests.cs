using FluentAssertions;
using Neoron.API.DTOs;
using Neoron.API.Models;
using Neoron.API.Tests.Fixtures;
using Neoron.API.Tests.Builders;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Neoron.API.Tests.Controllers;

[Collection("Database")]
public class MessageControllerTests : IntegrationTestBase
{
    public MessageControllerTests(TestWebApplicationFactory<Program> factory) 
        : base(factory)
    {
        Cleanup();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMessage_WhenMessageExists_ReturnsMessage()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Test message")
            .Build();

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/messages/{message.MessageId}");
        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.MessageId.Should().Be(message.MessageId);
        result.Content.Should().Be(message.Content);
    }

    [Fact]
    public async Task GetMessage_WhenMessageDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/messages/999999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateMessage_WithValidData_ReturnsCreatedMessage()
    {
        // Arrange
        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Test message",
            MessageType = 0
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);
        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.MessageId.Should().Be(request.MessageId);
        result.Content.Should().Be(request.Content);

        // Verify in database
        var savedMessage = await DbContext.Messages.FindAsync((long)request.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.Content.Should().Be(request.Content);
    }

    [Fact]
    public async Task CreateMessage_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = new string('x', 2001), // Exceeds max length
            MessageType = 0
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateMessage_WithValidData_ReturnsUpdatedMessage()
    {
        // Arrange
        var message = new DiscordMessage
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Original content",
            MessageType = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        var request = new UpdateMessageRequest
        {
            Content = "Updated content",
            EmbeddedContent = "New embedded content"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/messages/{message.MessageId}", request);
        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Content.Should().Be(request.Content);
        result.EmbeddedContent.Should().Be(request.EmbeddedContent);

        // Verify in database
        var updatedMessage = await DbContext.Messages.FindAsync(message.MessageId);
        updatedMessage.Should().NotBeNull();
        updatedMessage!.Content.Should().Be(request.Content);
        updatedMessage.EmbeddedContent.Should().Be(request.EmbeddedContent);
    }

    [Fact]
    public async Task DeleteMessage_WhenExists_ReturnsNoContent()
    {
        // Arrange
        var message = new DiscordMessage
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Test message",
            MessageType = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/messages/{message.MessageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify in database
        var deletedMessage = await DbContext.Messages.FindAsync(message.MessageId);
        deletedMessage.Should().BeNull();
    }

    [Fact]
    public async Task GetMessagesByChannel_ReturnsCorrectMessages()
    {
        // Arrange
        var channelId = 987654321L;
        var messages = new[]
        {
            new DiscordMessage
            {
                MessageId = 111111111,
                ChannelId = channelId,
                GuildId = 11111111,
                AuthorId = 22222222,
                Content = "First message",
                MessageType = 0,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2)
            },
            new DiscordMessage
            {
                MessageId = 222222222,
                ChannelId = channelId,
                GuildId = 11111111,
                AuthorId = 22222222,
                Content = "Second message",
                MessageType = 0,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
            }
        };

        await DbContext.Messages.AddRangeAsync(messages);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/messages/channel/{channelId}");
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(m => m.CreatedAt);
    }

    [Fact]
    public async Task GetMessagesByChannel_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var channelId = 987654321L;
        var messages = Enumerable.Range(1, 25).Select(i => new DiscordMessage
        {
            MessageId = i,
            ChannelId = channelId,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = $"Message {i}",
            MessageType = 0,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-i)
        }).ToArray();

        await DbContext.Messages.AddRangeAsync(messages);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/messages/channel/{channelId}?skip=5&take=10");
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Should().HaveCount(10);
        result.First().MessageId.Should().Be(6); // Should start from the 6th message
    }

    [Fact]
    public async Task CreateMessage_WithDuplicateMessageId_ReturnsBadRequest()
    {
        // Arrange
        var existingMessage = new DiscordMessage
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Existing message",
            MessageType = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await DbContext.Messages.AddAsync(existingMessage);
        await DbContext.SaveChangesAsync();

        var request = new CreateMessageRequest
        {
            MessageId = existingMessage.MessageId, // Same MessageId
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Duplicate message",
            MessageType = 0
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateMessage_WithInvalidContent_ReturnsBadRequest()
    {
        // Arrange
        var message = new DiscordMessage
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Original content",
            MessageType = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        var request = new UpdateMessageRequest
        {
            Content = "", // Empty content
            EmbeddedContent = "Some embedded content"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/messages/{message.MessageId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMessagesByChannel_WithInvalidPaginationParams_ReturnsBadRequest()
    {
        // Arrange
        var channelId = 987654321L;

        // Act
        var response = await Client.GetAsync($"/api/messages/channel/{channelId}?skip=-1&take=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateMessage_WithRateLimiting_ReturnsRateLimitError()
    {
        // Arrange
        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Test message",
            MessageType = 0
        };

        // Act - Send multiple requests quickly
        var tasks = Enumerable.Range(0, 10).Select(_ => 
            Client.PostAsJsonAsync("/api/messages", request));
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task UpdateMessage_WhenNotMessageAuthor_ReturnsForbidden()
    {
        // Arrange
        var message = new DiscordMessage
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Original content",
            MessageType = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        var request = new UpdateMessageRequest
        {
            Content = "Updated content"
        };

        // TODO: Set different user context
        // Act
        var response = await Client.PutAsJsonAsync($"/api/messages/{message.MessageId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
