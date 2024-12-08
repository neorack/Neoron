using FluentAssertions;
using Neoron.API.DTOs;
using Neoron.API.Models;
using Neoron.API.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Neoron.API.Tests.Controllers;

public class MessageControllerTests : IntegrationTestBase
{
    public MessageControllerTests(TestWebApplicationFactory<Program> factory) 
        : base(factory)
    {
        Cleanup();
    }

    [Fact]
    public async Task GetMessage_WhenMessageExists_ReturnsMessage()
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
}
