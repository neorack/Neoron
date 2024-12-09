using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Neoron.API.DTOs;
using Neoron.API.Tests.Builders;
using Neoron.API.Tests.Fixtures;
using Xunit;

namespace Neoron.API.Tests.Security;

[Collection("Database")]
public class AccessControlTests : IntegrationTestBase
{
    public AccessControlTests(TestWebApplicationFactory<Program> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMessage_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Clear();

        // Act
        var response = await Client.GetAsync("/api/messages/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMessage_WithInvalidRole_ReturnsForbidden()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-Roles", "invalid-role");

        // Act
        var response = await Client.GetAsync("/api/messages/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Roles");
    }

    [Fact]
    public async Task UpdateMessage_NonOwnerWithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithAuthorId(999)
            .Build();

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        var request = new UpdateMessageRequest { Content = "Updated content" };
        Client.DefaultRequestHeaders.Add("X-User-Id", "123");
        Client.DefaultRequestHeaders.Add("X-User-Roles", "user");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/messages/{message.MessageId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Remove("X-User-Roles");
    }

    [Fact]
    public async Task UpdateMessage_AdminRole_AllowsUpdate()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithAuthorId(999)
            .Build();

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        var request = new UpdateMessageRequest { Content = "Updated by admin" };
        Client.DefaultRequestHeaders.Add("X-User-Id", "123");
        Client.DefaultRequestHeaders.Add("X-User-Roles", "admin");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/messages/{message.MessageId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Remove("X-User-Roles");
    }

    [Fact]
    public async Task DeleteMessage_RequiresModeratorRole_ReturnsForbidden()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create().Build();
        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        Client.DefaultRequestHeaders.Add("X-User-Roles", "user");

        // Act
        var response = await Client.DeleteAsync($"/api/messages/{message.MessageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Roles");
    }

    [Fact]
    public async Task GetGuildMessages_RequiresGuildMembership_ReturnsForbidden()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-Id", "123");
        Client.DefaultRequestHeaders.Add("X-Guild-Memberships", "456,789"); // Different guild IDs

        // Act
        var response = await Client.GetAsync("/api/messages/guild/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Remove("X-Guild-Memberships");
    }

    [Fact]
    public async Task CreateMessage_RequiresChannelPermission_ReturnsForbidden()
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

        Client.DefaultRequestHeaders.Add("X-User-Id", "123");
        Client.DefaultRequestHeaders.Add("X-Channel-Permissions", "read"); // Missing write permission

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Remove("X-Channel-Permissions");
    }

    [Fact]
    public async Task BulkDelete_RequiresAdminRole_ReturnsForbidden()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-Roles", "moderator"); // Not admin

        // Act
        var response = await Client.DeleteAsync("/api/messages/bulk?ids=1,2,3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Roles");
    }

    [Fact]
    public async Task GetMessageHistory_RequiresAuditPermission_ReturnsForbidden()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-Roles", "user");

        // Act
        var response = await Client.GetAsync("/api/messages/123/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Roles");
    }
}
