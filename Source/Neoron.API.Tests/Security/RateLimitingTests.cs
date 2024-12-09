using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Neoron.API.DTOs;
using Neoron.API.Tests.Fixtures;
using Xunit;

namespace Neoron.API.Tests.Security;

[Collection("Database")]
public class RateLimitingTests : IntegrationTestBase
{
    public RateLimitingTests(TestWebApplicationFactory<Program> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateMessage_ExceedsUserRateLimit_ReturnsTooManyRequests()
    {
        // Arrange
        const string userId = "test-user-1";
        Client.DefaultRequestHeaders.Add("X-User-Id", userId);

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
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 10; i++)
        {
            request.MessageId += i;
            responses.Add(await Client.PostAsJsonAsync("/api/messages", request));
        }

        // Assert
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var tooManyRequestsResponse = responses.First(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        tooManyRequestsResponse.Headers.Should().ContainKey("Retry-After");

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
    }

    [Fact]
    public async Task CreateMessage_ExceedsChannelRateLimit_ReturnsTooManyRequests()
    {
        // Arrange
        const long channelId = 987654321;
        var requests = Enumerable.Range(0, 5).Select(i => new CreateMessageRequest
        {
            MessageId = 123456789 + i,
            ChannelId = channelId,
            GuildId = 11111111,
            AuthorId = 22222222 + i, // Different users
            Content = $"Test message {i}",
            MessageType = 0
        });

        // Act
        var responses = new List<HttpResponseMessage>();
        foreach (var request in requests)
        {
            responses.Add(await Client.PostAsJsonAsync("/api/messages", request));
        }

        // Assert
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task UpdateMessage_ExceedsGlobalRateLimit_ReturnsTooManyRequests()
    {
        // Arrange
        var request = new UpdateMessageRequest { Content = "Updated content" };
        const int requestCount = 20;

        // Act
        var tasks = Enumerable.Range(0, requestCount)
            .Select(_ => Client.PutAsJsonAsync("/api/messages/123", request));
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetMessages_WithValidRateLimit_SucceedsWithinLimit()
    {
        // Arrange
        const string userId = "test-user-2";
        Client.DefaultRequestHeaders.Add("X-User-Id", userId);

        // Act
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 3; i++) // Within rate limit
        {
            responses.Add(await Client.GetAsync("/api/messages"));
            await Task.Delay(1000); // Delay between requests
        }

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
    }

    [Fact]
    public async Task RateLimit_ResetsAfterWindow_AllowsNewRequests()
    {
        // Arrange
        const string userId = "test-user-3";
        Client.DefaultRequestHeaders.Add("X-User-Id", userId);

        var request = new CreateMessageRequest
        {
            MessageId = 123456789,
            ChannelId = 987654321,
            GuildId = 11111111,
            AuthorId = 22222222,
            Content = "Test message",
            MessageType = 0
        };

        // Act - First batch of requests
        var firstBatchResponses = new List<HttpResponseMessage>();
        for (int i = 0; i < 5; i++)
        {
            request.MessageId += i;
            firstBatchResponses.Add(await Client.PostAsJsonAsync("/api/messages", request));
        }

        // Wait for rate limit window to reset
        await Task.Delay(5000);

        // Second batch of requests
        var secondBatchResponses = new List<HttpResponseMessage>();
        for (int i = 5; i < 10; i++)
        {
            request.MessageId += i;
            secondBatchResponses.Add(await Client.PostAsJsonAsync("/api/messages", request));
        }

        // Assert
        secondBatchResponses.Should().Contain(r => r.StatusCode == HttpStatusCode.Created);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
    }

    [Fact]
    public async Task RateLimit_DifferentEndpoints_TrackedSeparately()
    {
        // Arrange
        const string userId = "test-user-4";
        Client.DefaultRequestHeaders.Add("X-User-Id", userId);

        // Act - Exhaust rate limit for POST
        var postResponses = new List<HttpResponseMessage>();
        for (int i = 0; i < 5; i++)
        {
            var request = new CreateMessageRequest
            {
                MessageId = 123456789 + i,
                ChannelId = 987654321,
                GuildId = 11111111,
                AuthorId = 22222222,
                Content = "Test message",
                MessageType = 0
            };
            postResponses.Add(await Client.PostAsJsonAsync("/api/messages", request));
        }

        // GET request should still work
        var getResponse = await Client.GetAsync("/api/messages");

        // Assert
        postResponses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
    }

    [Fact]
    public async Task RateLimit_Headers_ContainCorrectInformation()
    {
        // Arrange
        const string userId = "test-user-5";
        Client.DefaultRequestHeaders.Add("X-User-Id", userId);

        // Act
        var response = await Client.GetAsync("/api/messages");

        // Assert
        response.Headers.Should().ContainKey("X-RateLimit-Limit");
        response.Headers.Should().ContainKey("X-RateLimit-Remaining");
        response.Headers.Should().ContainKey("X-RateLimit-Reset");

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
    }

    [Fact]
    public async Task RateLimit_BurstRequests_HandledCorrectly()
    {
        // Arrange
        const string userId = "test-user-6";
        Client.DefaultRequestHeaders.Add("X-User-Id", userId);

        // Act - Send burst of concurrent requests
        var tasks = Enumerable.Range(0, 10).Select(_ => Client.GetAsync("/api/messages"));
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests).Should().BeGreaterThan(0);

        // Cleanup
        Client.DefaultRequestHeaders.Remove("X-User-Id");
    }
}
