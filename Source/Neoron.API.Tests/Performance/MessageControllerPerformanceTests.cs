using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
        Cleanup().GetAwaiter().GetResult();
    }

    private async Task Cleanup()
    {
        var messages = await DbContext.DiscordMessages.ToListAsync();
        DbContext.DiscordMessages.RemoveRange(messages);
        await DbContext.SaveChangesAsync();
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
    public async Task MemoryLeakTest_RepeatedOperations_StableMemoryUsage()
    {
        // Arrange
        const int iterations = 100;
        var initialMemory = GC.GetTotalMemory(true);
        var memoryReadings = new List<long>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var message = new DiscordMessageBuilder().WithRandomData().Build();
            await Client.PostAsJsonAsync("/api/messages", message);
            
            if (i % 10 == 0) // Take memory reading every 10 iterations
            {
                GC.Collect();
                var currentMemory = GC.GetTotalMemory(true);
                memoryReadings.Add(currentMemory);
            }
        }

        // Assert
        var memoryGrowth = memoryReadings.Last() - memoryReadings.First();
        var averageGrowth = memoryGrowth / (memoryReadings.Count - 1);
        
        _output.WriteLine($"Initial Memory: {initialMemory / 1024 / 1024}MB");
        _output.WriteLine($"Final Memory: {memoryReadings.Last() / 1024 / 1024}MB");
        _output.WriteLine($"Average Growth: {averageGrowth / 1024}KB per 10 operations");

        // Memory growth should be relatively stable
        averageGrowth.Should().BeLessThan(1024 * 1024); // Less than 1MB growth per 10 operations
    }

    [Fact]
    public async Task RateLimiting_ExceedsThreshold_ThrottlesRequests()
    {
        // Arrange
        const int requestCount = 100;
        const int expectedRateLimit = 10; // requests per second
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(_fixture.Client.GetAsync($"/api/messages/{TestUtils.GenerateRandomId()}"));
        }
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var throttledCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var successCount = responses.Count(r => r.IsSuccessStatusCode);

        _output.WriteLine($"Total requests: {requestCount}");
        _output.WriteLine($"Throttled requests: {throttledCount}");
        _output.WriteLine($"Successful requests: {successCount}");
        _output.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");

        throttledCount.Should().BeGreaterThan(0);
        (successCount / (double)requestCount).Should().BeLessThan(1.0);
    }

    [Fact]
    public async Task DatabaseResilience_TemporaryFailure_Recovers()
    {
        // Arrange
        var message = new DiscordMessageBuilder().WithRandomData().Build();
        var dbContext = _fixture.Factory.Services.GetRequiredService<ApplicationDbContext>();
        
        // Simulate connection issue by disposing context
        await dbContext.DisposeAsync();

        // Act & Assert
        await TestUtils.RetryAsync(async () =>
        {
            var response = await _fixture.Client.PostAsJsonAsync("/api/messages", message);
            response.IsSuccessStatusCode.Should().BeTrue();
        }, maxAttempts: 3);
    }

    [Fact]
    public async Task ConcurrentUpdates_OptimisticConcurrency_HandlesConflicts()
    {
        // Arrange
        var message = await TestUtils.TestDataSeeder.SeedTestMessages(_fixture.DbContext, 1);
        var messageId = message.First().MessageId;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            var updateContent = new UpdateMessageRequest { Content = $"Concurrent update {i}" };
            tasks.Add(_fixture.Client.PutAsJsonAsync($"/api/messages/{messageId}", updateContent));
        }
        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        _output.WriteLine($"Successful updates: {successCount}");
        _output.WriteLine($"Conflict responses: {conflictCount}");

        successCount.Should().Be(1);
        conflictCount.Should().Be(4);
    }

    [Fact]
    public async Task MessageValidation_UnderLoad_MaintainsIntegrity()
    {
        // Arrange
        const int messageCount = 100;
        var validMessages = Enumerable.Range(0, messageCount / 2)
            .Select(_ => new DiscordMessageBuilder().WithRandomData().Build());
        var invalidMessages = Enumerable.Range(0, messageCount / 2)
            .Select(_ => new DiscordMessageBuilder().WithContent("").Build());
        var allMessages = validMessages.Concat(invalidMessages).ToList();

        // Act
        var tasks = allMessages.Select(msg => 
            _fixture.Client.PostAsJsonAsync("/api/messages", msg));
        var responses = await Task.WhenAll(tasks);

        // Assert
        var validCount = responses.Count(r => r.IsSuccessStatusCode);
        var invalidCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

        _output.WriteLine($"Valid responses: {validCount}");
        _output.WriteLine($"Invalid responses: {invalidCount}");

        validCount.Should().Be(messageCount / 2);
        invalidCount.Should().Be(messageCount / 2);
    }

    [Fact]
    public async Task ThreadMessages_LargeThread_PerformsEfficiently()
    {
        // Arrange
        const int threadSize = 500;
        var threadParent = new DiscordMessageBuilder().WithRandomData().Build();
        await _fixture.DbContext.DiscordMessages.AddAsync(threadParent);
        await _fixture.DbContext.SaveChangesAsync();

        var threadMessages = Enumerable.Range(0, threadSize)
            .Select(_ => new DiscordMessageBuilder()
                .WithRandomData()
                .InThread(threadParent.MessageId)
                .Build());
        await _fixture.DbContext.DiscordMessages.AddRangeAsync(threadMessages);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var metrics = await TestUtils.PerformanceMetrics.MeasureOperation(async () =>
        {
            var response = await _fixture.Client.GetAsync($"/api/messages/thread/{threadParent.MessageId}");
            response.IsSuccessStatusCode.Should().BeTrue();
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>();
            result.Should().HaveCount(threadSize);
        });

        // Assert
        _output.WriteLine($"Duration: {metrics.Duration.TotalMilliseconds}ms");
        _output.WriteLine($"Memory used: {metrics.MemoryUsed / 1024 / 1024}MB");

        metrics.Duration.Should().BeLessThan(TimeSpan.FromSeconds(2));
        metrics.MemoryUsed.Should().BeLessThan(100 * 1024 * 1024); // 100MB
    }

    private async Task CleanupTestData()
    {
        var messages = await _fixture.DbContext.DiscordMessages.ToListAsync();
        _fixture.DbContext.DiscordMessages.RemoveRange(messages);
        await _fixture.DbContext.SaveChangesAsync();
    }

    public async Task InitializeAsync()
    {
        await CleanupTestData();
    }

    public async Task DisposeAsync()
    {
        await CleanupTestData();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task ConcurrentOperations_MixedRequests_HandlesEfficiently(int concurrentRequests)
    {
        // Arrange
        var stopwatch = new Stopwatch();
        var random = new Random();
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        stopwatch.Start();
        for (int i = 0; i < concurrentRequests; i++)
        {
            var task = random.Next(3) switch
            {
                0 => Client.GetAsync($"/api/messages/{TestUtils.GenerateRandomId()}"),
                1 => Client.PostAsJsonAsync("/api/messages", 
                    new DiscordMessageBuilder().WithRandomData().Build()),
                _ => Client.DeleteAsync($"/api/messages/{TestUtils.GenerateRandomId()}")
            };
            tasks.Add(task);
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var responses = tasks.Select(t => t.Result).ToList();
        var successRate = (double)responses.Count(r => 
            r.StatusCode == HttpStatusCode.OK || 
            r.StatusCode == HttpStatusCode.Created || 
            r.StatusCode == HttpStatusCode.NotFound) / concurrentRequests;

        _output.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per request: {stopwatch.ElapsedMilliseconds / concurrentRequests}ms");
        _output.WriteLine($"Success rate: {successRate:P}");

        successRate.Should().BeGreaterThan(0.95);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(concurrentRequests * 50); // 50ms per request max
    }

    [Fact]
    public async Task DatabasePerformance_BulkOperations_HandlesEfficiently()
    {
        // Arrange
        const int batchSize = 1000;
        var messages = DiscordMessageBuilder.CreateMany(batchSize).ToList();
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        foreach (var batch in messages.Chunk(100))
        {
            var tasks = batch.Select(msg => 
                Client.PostAsJsonAsync("/api/messages", msg));
            await Task.WhenAll(tasks);
        }
        stopwatch.Stop();

        // Assert
        var insertedCount = await DbContext.DiscordMessages
            .CountAsync(m => messages.Select(msg => msg.MessageId)
            .Contains(m.MessageId));

        _output.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per message: {stopwatch.ElapsedMilliseconds / batchSize}ms");
        _output.WriteLine($"Messages per second: {batchSize / (stopwatch.ElapsedMilliseconds / 1000.0):F2}");

        insertedCount.Should().Be(batchSize);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
    }

    [Fact]
    public async Task ResponseTimeDistribution_UnderLoad_MeetsPerformanceTargets()
    {
        // Arrange
        const int sampleSize = 1000;
        var responseTimes = new List<long>();
        var message = new DiscordMessageBuilder().WithRandomData().Build();

        // Act
        for (int i = 0; i < sampleSize; i++)
        {
            var sw = Stopwatch.StartNew();
            await Client.PostAsJsonAsync("/api/messages", message);
            sw.Stop();
            responseTimes.Add(sw.ElapsedMilliseconds);

            // Small delay to prevent overwhelming the server
            await Task.Delay(10);
        }

        // Calculate percentiles
        responseTimes.Sort();
        var p50 = responseTimes[sampleSize / 2];
        var p90 = responseTimes[(int)(sampleSize * 0.9)];
        var p99 = responseTimes[(int)(sampleSize * 0.99)];

        // Assert
        _output.WriteLine($"P50: {p50}ms");
        _output.WriteLine($"P90: {p90}ms");
        _output.WriteLine($"P99: {p99}ms");
        _output.WriteLine($"Max: {responseTimes.Max()}ms");

        p50.Should().BeLessThan(100);  // 50th percentile under 100ms
        p90.Should().BeLessThan(200);  // 90th percentile under 200ms
        p99.Should().BeLessThan(500);  // 99th percentile under 500ms
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
