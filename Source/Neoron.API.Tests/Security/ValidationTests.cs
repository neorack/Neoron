using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Neoron.API.DTOs;
using Neoron.API.Tests.Fixtures;
using Xunit;

namespace Neoron.API.Tests.Security;

[Collection("Database")]
public class ValidationTests : IntegrationTestBase
{
    public ValidationTests(TestWebApplicationFactory<Program> factory) 
        : base(factory)
    {
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("data:text/html;base64,PHNjcmlwdD5hbGVydCgneHNzJyk8L3NjcmlwdD4=")]
    public async Task CreateMessage_WithPotentialXssContent_ReturnsBadRequest(string maliciousContent)
    {
        // Arrange
        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = maliciousContent,
            MessageType = 0
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("' OR '1'='1")]
    [InlineData("; DROP TABLE Messages;--")]
    [InlineData("UNION SELECT * FROM Users")]
    public async Task CreateMessage_WithSqlInjectionAttempt_ReturnsBadRequest(string sqlInjectionContent)
    {
        // Arrange
        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = sqlInjectionContent,
            MessageType = 0
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateMessage_WithInvalidContent_ReturnsBadRequest(string invalidContent)
    {
        // Arrange
        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = invalidContent,
            MessageType = 0
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateMessage_WithOverlongContent_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = new string('x', 2001), // Discord max length is 2000
            MessageType = 0
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task CreateMessage_WithInvalidMessageType_ReturnsBadRequest(int invalidMessageType)
    {
        // Arrange
        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Test content",
            MessageType = invalidMessageType
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateMessage_WithHtmlEncodedContent_ProcessesCorrectly()
    {
        // Arrange
        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Test &lt;script&gt; content",
            MessageType = 0
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);
        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Content.Should().Be("Test &lt;script&gt; content");
    }
}
