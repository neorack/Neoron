using System.Diagnostics;
using System.Net.Http.Json;
using FluentAssertions;
using Neoron.API.DTOs;
using Neoron.API.Models;
using Neoron.API.Tests.Builders;
using Neoron.API.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Neoron.API.Tests.Performance;

[Collection("Database")]
public class MessageControllerPerformanceTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    
    public MessageControllerPerformanceTests(
        TestWebApplicationFactory<Program> factory,
        ITestOutputHelper output) : base(factory)
    {
        _output = output;
        Cleanup();
    }

    [Fact]
    public async Task LoadTest_GetMessages_HandlesMultipleRequests()
    {
        // Arrange
        const int numberOfMessages = 100;
        const int concurrentRequests = 10;
        var messages = GenerateTestMessages(numberOfMessages);
        await SeedMessages(messages);

        var stopwatch = new Stopwatch();
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        stopwatch.Start();
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(Client.GetAsync($"/api/messages/channel/{messages[0].ChannelId}"));
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var responses = tasks.Select(t => t.Result).ToList();
        responses.Should().AllSatisfy(r => r.IsSuccessStatusCode.Should().BeTrue());

        _output.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per request: {stopwatch.ElapsedMilliseconds / concurrentRequests}ms");

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 5 seconds max
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task StressTest_CreateMessages_HandlesHighVolume(int numberOfRequests)
    {
        // Arrange
        var stopwatch = new Stopwatch();
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        stopwatch.Start();
        for (int i = 0; i < numberOfRequests; i++)
        {
            var request = new CreateMessageRequest
            {
                MessageId = 1000000 + i,
                ChannelId = 1,
                GuildId = 1,
                AuthorId = 1,
                Content = $"Stress test message {i}",
                MessageType = 0
            };
            tasks.Add(Client.PostAsJsonAsync("/api/messages", request));
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var responses = tasks.Select(t => t.Result).ToList();
        var successRate = (double)responses.Count(r => r.IsSuccessStatusCode) / numberOfRequests;

        _output.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per request: {stopwatch.ElapsedMilliseconds / numberOfRequests}ms");
        _output.WriteLine($"Success rate: {successRate:P}");

        successRate.Should().BeGreaterThan(0.95); // 95% success rate minimum
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(numberOfRequests * 100); // Average 100ms per request
    }

    [Fact]
    public async Task MemoryTest_LargeDataSet_HandlesEfficiently()
    {
        // Arrange
        const int numberOfMessages = 1000;
        var messages = GenerateTestMessages(numberOfMessages);
        await SeedMessages(messages);

        var initialMemory = GC.GetTotalMemory(true);
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        var response = await Client.GetAsync($"/api/messages/channel/{messages[0].ChannelId}");
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>();
        stopwatch.Stop();

        var finalMemory = GC.GetTotalMemory(false);
        var memoryUsed = finalMemory - initialMemory;

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        result.Should().NotBeNull();

        _output.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Memory used: {memoryUsed / 1024 / 1024:F2}MB");

        memoryUsed.Should().BeLessThan(50 * 1024 * 1024); // Less than 50MB memory increase
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Less than 1 second
    }

    private static List<DiscordMessage> GenerateTestMessages(int count)
    {
        var messages = new List<DiscordMessage>();
        for (int i = 0; i < count; i++)
        {
            messages.Add(DiscordMessageBuilder.Create()
                .WithMessageId(i + 1)
                .WithChannelId(1)
                .WithGuildId(1)
                .WithAuthorId(1)
                .WithContent($"Test message {i}")
                .Build());
        }
        return messages;
    }

    private async Task SeedMessages(IEnumerable<DiscordMessage> messages)
    {
        await DbContext.Messages.AddRangeAsync(messages);
        await DbContext.SaveChangesAsync();
    }
}
