using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Neoron.API.DTOs;
using Neoron.API.Models;
using Neoron.API.Tests.Builders;
using Neoron.API.Tests.Fixtures;
using Xunit;

namespace Neoron.API.Tests.Infrastructure;

[Collection("Database")]
public class CachingTests : IntegrationTestBase
{
    private readonly IDistributedCache _cache;

    public CachingTests(TestWebApplicationFactory<Program> factory) 
        : base(factory)
    {
        _cache = factory.Services.GetRequiredService<IDistributedCache>();
        Cleanup();
    }

    [Fact]
    public async Task GetMessage_WhenCached_ReturnsCachedResult()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Test message")
            .Build();

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        // First request to cache the result
        await Client.GetAsync($"/api/messages/{message.MessageId}");

        // Modify the message in the database directly
        message.Content = "Updated content";
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/messages/{message.MessageId}");
        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Content.Should().Be("Test message"); // Should return cached content
        response.Headers.Should().ContainKey("X-Cache-Hit");
    }

    [Fact]
    public async Task UpdateMessage_InvalidatesCachedData()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Original content")
            .Build();

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        // Cache the original message
        await Client.GetAsync($"/api/messages/{message.MessageId}");

        // Act
        var updateRequest = new UpdateMessageRequest { Content = "Updated content" };
        await Client.PutAsJsonAsync($"/api/messages/{message.MessageId}", updateRequest);

        // Get the message again
        var response = await Client.GetAsync($"/api/messages/{message.MessageId}");
        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();

        // Assert
        result.Should().NotBeNull();
        result!.Content.Should().Be("Updated content"); // Should return new content
        response.Headers.Should().NotContainKey("X-Cache-Hit");
    }

    [Fact]
    public async Task DeleteMessage_RemovesFromCache()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Test message")
            .Build();

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        // Cache the message
        await Client.GetAsync($"/api/messages/{message.MessageId}");

        // Act
        await Client.DeleteAsync($"/api/messages/{message.MessageId}");

        // Try to get from cache
        var cacheKey = $"message:{message.MessageId}";
        var cachedValue = await _cache.GetAsync(cacheKey);

        // Assert
        cachedValue.Should().BeNull();
    }

    [Fact]
    public async Task GetMessages_WithPagination_CachesSeparatePages()
    {
        // Arrange
        var messages = Enumerable.Range(1, 50)
            .Select(i => DiscordMessageBuilder.Create()
                .WithMessageId(i)
                .WithContent($"Message {i}")
                .WithChannelId(1)
                .Build())
            .ToList();

        await DbContext.Messages.AddRangeAsync(messages);
        await DbContext.SaveChangesAsync();

        // Act
        // Request first page
        var firstPageResponse1 = await Client.GetAsync("/api/messages/channel/1?skip=0&take=10");
        var firstPageResult1 = await firstPageResponse1.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>();

        // Request second page
        var secondPageResponse = await Client.GetAsync("/api/messages/channel/1?skip=10&take=10");
        var secondPageResult = await secondPageResponse.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>();

        // Request first page again
        var firstPageResponse2 = await Client.GetAsync("/api/messages/channel/1?skip=0&take=10");
        var firstPageResult2 = await firstPageResponse2.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>();

        // Assert
        firstPageResponse2.Headers.Should().ContainKey("X-Cache-Hit");
        firstPageResult1.Should().BeEquivalentTo(firstPageResult2);
        secondPageResult.Should().NotBeEquivalentTo(firstPageResult1);
    }

    [Fact]
    public async Task CacheExpiration_ReturnsUpdatedData()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Test message")
            .Build();

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        // Cache the message
        await Client.GetAsync($"/api/messages/{message.MessageId}");

        // Wait for cache to expire (using short expiration for test)
        await Task.Delay(2000); // Assuming 1 second cache duration for tests

        // Update the message in the database
        message.Content = "Updated after cache expiration";
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/messages/{message.MessageId}");
        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();

        // Assert
        result.Should().NotBeNull();
        result!.Content.Should().Be("Updated after cache expiration");
        response.Headers.Should().NotContainKey("X-Cache-Hit");
    }

    [Fact]
    public async Task BulkUpdate_InvalidatesMultipleCacheEntries()
    {
        // Arrange
        var messages = Enumerable.Range(1, 3)
            .Select(i => DiscordMessageBuilder.Create()
                .WithMessageId(i)
                .WithContent($"Message {i}")
                .Build())
            .ToList();

        await DbContext.Messages.AddRangeAsync(messages);
        await DbContext.SaveChangesAsync();

        // Cache all messages
        foreach (var message in messages)
        {
            await Client.GetAsync($"/api/messages/{message.MessageId}");
        }

        // Act
        var bulkUpdateRequest = new BulkUpdateRequest
        {
            MessageIds = messages.Select(m => m.MessageId).ToList(),
            Content = "Bulk updated content"
        };
        await Client.PutAsJsonAsync("/api/messages/bulk", bulkUpdateRequest);

        // Get all messages again
        var responses = await Task.WhenAll(
            messages.Select(m => Client.GetAsync($"/api/messages/{m.MessageId}")));
        var results = await Task.WhenAll(
            responses.Select(r => r.Content.ReadFromJsonAsync<MessageResponse>()));

        // Assert
        results.Should().AllSatisfy(r =>
        {
            r.Should().NotBeNull();
            r!.Content.Should().Be("Bulk updated content");
        });
        responses.Should().AllSatisfy(r => 
            r.Headers.Should().NotContainKey("X-Cache-Hit"));
    }

    [Fact]
    public async Task ConcurrentRequests_HandlesCacheCorrectly()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Test message")
            .Build();

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Client.GetAsync($"/api/messages/{message.MessageId}"));
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Skip(1).Should().AllSatisfy(r => 
            r.Headers.Should().ContainKey("X-Cache-Hit"));
    }
}
