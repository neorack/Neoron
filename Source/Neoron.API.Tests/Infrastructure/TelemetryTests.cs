using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Neoron.API.DTOs;
using Neoron.API.Tests.Builders;
using Neoron.API.Tests.Fixtures;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Xunit;
using Xunit.Abstractions;

namespace Neoron.API.Tests.Infrastructure;

[Collection("Database")]
public class TelemetryTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly TracerProvider _tracerProvider;
    private readonly List<Activity> _exportedActivities;

    public TelemetryTests(
        TestWebApplicationFactory<Program> factory,
        ITestOutputHelper output) : base(factory)
    {
        _output = output;
        _exportedActivities = new List<Activity>();

        // Configure test trace provider
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddInMemoryExporter(_exportedActivities)
            .Build()!;

        Cleanup();
    }

    [Fact]
    public async Task RequestProcessing_GeneratesExpectedTraces()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Test message")
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify traces
        _exportedActivities.Should().Contain(a => 
            a.DisplayName.Contains("POST") && 
            a.DisplayName.Contains("/api/messages"));

        _exportedActivities.Should().Contain(a => 
            a.DisplayName.Contains("SaveChanges") || 
            a.DisplayName.Contains("ExecuteReader"));

        // Log trace details
        foreach (var activity in _exportedActivities)
        {
            _output.WriteLine($"Activity: {activity.DisplayName}");
            _output.WriteLine($"Duration: {activity.Duration}");
            foreach (var tag in activity.Tags)
            {
                _output.WriteLine($"Tag: {tag.Key} = {tag.Value}");
            }
        }
    }

    [Fact]
    public async Task ErrorHandling_GeneratesErrorTraces()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var response = await Client.GetAsync($"/api/messages/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify error traces
        var errorActivity = _exportedActivities.Should().Contain(a => 
            a.Status == ActivityStatusCode.Error).Which;

        errorActivity.Tags.Should().Contain(t => 
            t.Key == "error.type" && 
            t.Value!.ToString()!.Contains("BadRequest"));
    }

    [Fact]
    public async Task DatabaseOperations_TracksQueryPerformance()
    {
        // Arrange
        var messages = Enumerable.Range(1, 100)
            .Select(i => DiscordMessageBuilder.Create()
                .WithMessageId(i)
                .WithContent($"Message {i}")
                .Build())
            .ToList();

        await DbContext.Messages.AddRangeAsync(messages);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/messages/channel/1?take=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify database operation traces
        var dbActivity = _exportedActivities.Should().Contain(a => 
            a.DisplayName.Contains("ExecuteReader")).Which;

        dbActivity.Duration.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ConcurrentRequests_TracksParallelOperations()
    {
        // Arrange
        const int concurrentRequests = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            var message = DiscordMessageBuilder.Create()
                .WithMessageId(i)
                .WithContent($"Message {i}")
                .Build();

            tasks.Add(Client.PostAsJsonAsync("/api/messages", message));
        }

        await Task.WhenAll(tasks);

        // Assert
        var requestActivities = _exportedActivities.Where(a => 
            a.DisplayName.Contains("POST") && 
            a.DisplayName.Contains("/api/messages")).ToList();

        requestActivities.Count.Should().Be(concurrentRequests);

        // Verify parallel processing
        var timeRanges = requestActivities
            .Select(a => new { Start = a.StartTimeUtc, End = a.StartTimeUtc + a.Duration })
            .OrderBy(t => t.Start)
            .ToList();

        // Check for overlapping requests
        for (int i = 1; i < timeRanges.Count; i++)
        {
            timeRanges[i].Start.Should().BeBefore(timeRanges[i - 1].End);
        }
    }

    [Fact]
    public async Task Authentication_TracksSecurityEvents()
    {
        // Arrange
        Client.DefaultRequestHeaders.Remove("Authorization");

        // Act
        var response = await Client.GetAsync("/api/messages/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Verify security traces
        _exportedActivities.Should().Contain(a => 
            a.Tags.Any(t => 
                t.Key == "security.event" && 
                t.Value!.ToString()!.Contains("unauthorized")));
    }

    [Fact]
    public async Task RateLimiting_TracksThrottlingEvents()
    {
        // Arrange
        const int requestCount = 20;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(Client.GetAsync("/api/messages/1"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        // Verify rate limiting traces
        _exportedActivities.Should().Contain(a => 
            a.Tags.Any(t => 
                t.Key == "rate.limit.exceeded" && 
                t.Value!.ToString() == "true"));
    }

    public override void Dispose()
    {
        _tracerProvider?.Dispose();
        base.Dispose();
    }
}
