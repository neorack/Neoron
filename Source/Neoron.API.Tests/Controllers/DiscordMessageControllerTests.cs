using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Neoron.API.Models;
using Neoron.API.Tests.Builders;
using Neoron.API.Tests.Fixtures;
using Neoron.API.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Neoron.API.Tests.Controllers;

[Collection("TestApi")]
[Trait("Category", TestCategories.Controllers)]
public class DiscordMessageControllerTests
{
    private readonly TestCollectionFixture _fixture;

    public DiscordMessageControllerTests(TestCollectionFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetMessage_ReturnsCorrectMessage()
    {
        // Arrange
        var message = await TestUtils.TestDataSeeder.SeedTestMessages(_fixture.DbContext, 1);

        // Act
        var response = await _fixture.Client.GetAsync($"/api/messages/{message.First().MessageId}");

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<DiscordMessage>();
        result.Should().NotBeNull();
        result!.MessageId.Should().Be(message.First().MessageId);
    }

    [Fact]
    public async Task GetMessage_NonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _fixture.Client.GetAsync($"/api/messages/{long.MaxValue}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateMessage_ValidMessage_ReturnsCreated()
    {
        // Arrange
        var message = new DiscordMessageBuilder()
            .WithContent("Test Message")
            .Build();

        // Act
        var response = await _fixture.Client.PostAsJsonAsync("/api/messages", message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<DiscordMessage>();
        result.Should().NotBeNull();
        result!.Content.Should().Be("Test Message");
    }

    [Fact]
    public async Task UpdateMessage_ValidUpdate_ReturnsSuccess()
    {
        // Arrange
        var message = await TestUtils.TestDataSeeder.SeedTestMessages(_fixture.DbContext, 1);
        var updateRequest = new UpdateMessageRequest 
        { 
            Content = "Updated Content" 
        };

        // Act
        var response = await _fixture.Client.PutAsJsonAsync(
            $"/api/messages/{message.First().MessageId}", 
            updateRequest);

        // Assert
        response.Should().BeSuccessful();
        var updated = await _fixture.DbContext.DiscordMessages
            .FindAsync(message.First().MessageId);
        updated.Should().NotBeNull();
        updated!.Content.Should().Be("Updated Content");
    }

    [Fact]
    public async Task DeleteMessage_ExistingMessage_ReturnsSuccess()
    {
        // Arrange
        var message = await TestUtils.TestDataSeeder.SeedTestMessages(_fixture.DbContext, 1);

        // Act
        var response = await _fixture.Client.DeleteAsync($"/api/messages/{message.First().MessageId}");

        // Assert
        response.Should().BeSuccessful();
        var deleted = await _fixture.DbContext.DiscordMessages
            .FindAsync(message.First().MessageId);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetThreadMessages_ReturnsCorrectMessages()
    {
        // Arrange
        await TestUtils.TestDataSeeder.SeedTestThreads(_fixture.DbContext);
        var threadParent = await _fixture.DbContext.DiscordMessages
            .FirstAsync(m => m.ThreadMessages.Any());

        // Act
        var response = await _fixture.Client.GetAsync($"/api/messages/thread/{threadParent.MessageId}");

        // Assert
        response.Should().BeSuccessful();
        var messages = await response.Content.ReadFromJsonAsync<IEnumerable<DiscordMessage>>();
        messages.Should().NotBeNull();
        messages!.Should().AllSatisfy(m => m.ThreadId.Should().Be(threadParent.MessageId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateMessage_InvalidContent_ReturnsBadRequest(string content)
    {
        // Arrange
        var message = new DiscordMessageBuilder()
            .WithContent(content)
            .Build();

        // Act
        var response = await _fixture.Client.PostAsJsonAsync("/api/messages", message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMessagesByChannel_ReturnsPaginatedResults()
    {
        // Arrange
        const long channelId = 123;
        await TestUtils.TestDataSeeder.SeedTestMessages(_fixture.DbContext, 20);

        // Act
        var response = await _fixture.Client.GetAsync($"/api/messages/channel/{channelId}?page=1&pageSize=5");

        // Assert
        response.Should().BeSuccessful();
        var messages = await response.Content.ReadFromJsonAsync<IEnumerable<DiscordMessage>>();
        messages.Should().NotBeNull();
        messages!.Count().Should().Be(5);
    }

    [Fact]
    public async Task UpdateMessage_ConcurrentUpdate_ReturnsConflict()
    {
        // Arrange
        var message = await TestUtils.TestDataSeeder.SeedTestMessages(_fixture.DbContext, 1);
        var firstUpdate = new UpdateMessageRequest { Content = "First Update" };
        var secondUpdate = new UpdateMessageRequest { Content = "Second Update" };

        // Act
        var response1 = await _fixture.Client.PutAsJsonAsync(
            $"/api/messages/{message.First().MessageId}", 
            firstUpdate);
        var response2 = await _fixture.Client.PutAsJsonAsync(
            $"/api/messages/{message.First().MessageId}", 
            secondUpdate);

        // Assert
        response1.Should().BeSuccessful();
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
