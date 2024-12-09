using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Neoron.API.DTOs;
using Neoron.API.Tests.Builders;
using Neoron.API.Tests.Fixtures;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Xunit;
using Xunit.Abstractions;

namespace Neoron.API.Tests.Infrastructure;

[Collection("Database")]
public class MetricsTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly MeterProvider _meterProvider;
    private readonly List<Measurement<long>> _measurements;
    private readonly List<Measurement<double>> _timingMeasurements;

    public MetricsTests(
        TestWebApplicationFactory<Program> factory,
        ITestOutputHelper output) : base(factory)
    {
        _output = output;
        _measurements = new List<Measurement<long>>();
        _timingMeasurements = new List<Measurement<double>>();

        // Configure metrics collection
        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("Neoron.API")
            .AddInMemoryExporter(_measurements, _timingMeasurements)
            .Build()!;

        Cleanup();
    }

    [Fact]
    public async Task MessageCreation_TracksMessageMetrics()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Test message")
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify metrics
        _measurements.Should().Contain(m => 
            m.MetricName == "messages.created" && 
            m.Value == 1);

        _measurements.Should().Contain(m =>
            m.MetricName == "messages.total" &&
            m.Value > 0);

        // Log metrics
        foreach (var measurement in _measurements)
        {
            _output.WriteLine($"Metric: {measurement.MetricName} = {measurement.Value}");
            foreach (var tag in measurement.Tags)
            {
                _output.WriteLine($"Tag: {tag.Key} = {tag.Value}");
            }
        }
    }

    [Fact]
    public async Task MessageProcessing_TracksPerformanceMetrics()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Test message")
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync("/api/messages", message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify timing metrics
        _timingMeasurements.Should().Contain(m =>
            m.MetricName == "message.processing.duration" &&
            m.Value < 1000); // Less than 1 second

        // Log timing metrics
        foreach (var measurement in _timingMeasurements)
        {
            _output.WriteLine($"Timing: {measurement.MetricName} = {measurement.Value}ms");
        }
    }

    [Fact]
    public async Task DatabaseOperations_TracksQueryMetrics()
    {
        // Arrange
        const int messageCount = 100;
        var messages = Enumerable.Range(1, messageCount)
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

        // Verify database metrics
        _measurements.Should().Contain(m =>
            m.MetricName == "database.queries" &&
            m.Value > 0);

        _timingMeasurements.Should().Contain(m =>
            m.MetricName == "database.query.duration" &&
            m.Value < 1000);
    }

    [Fact]
    public async Task ErrorHandling_TracksErrorMetrics()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var response = await Client.GetAsync($"/api/messages/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify error metrics
        _measurements.Should().Contain(m =>
            m.MetricName == "errors.total" &&
            m.Value > 0);

        _measurements.Should().Contain(m =>
            m.MetricName == "errors.by.type" &&
            m.Tags.Contains(new KeyValuePair<string, string>("error_type", "validation")));
    }

    [Fact]
    public async Task RateLimiting_TracksThrottlingMetrics()
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

        // Verify rate limiting metrics
        _measurements.Should().Contain(m =>
            m.MetricName == "rate.limits.exceeded" &&
            m.Value > 0);
    }

    [Fact]
    public async Task CacheOperations_TracksCacheMetrics()
    {
        // Arrange
        var message = DiscordMessageBuilder.Create()
            .WithContent("Test message")
            .Build();

        await DbContext.Messages.AddAsync(message);
        await DbContext.SaveChangesAsync();

        // Act - First request (cache miss)
        var response1 = await Client.GetAsync($"/api/messages/{message.MessageId}");
        // Second request (cache hit)
        var response2 = await Client.GetAsync($"/api/messages/{message.MessageId}");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify cache metrics
        _measurements.Should().Contain(m =>
            m.MetricName == "cache.hits" &&
            m.Value > 0);

        _measurements.Should().Contain(m =>
            m.MetricName == "cache.misses" &&
            m.Value > 0);
    }

    [Fact]
    public async Task ResourceUsage_TracksSystemMetrics()
    {
        // Arrange & Act
        var initialMemory = GC.GetTotalMemory(true);
        
        // Generate some load
        var tasks = Enumerable.Range(1, 10).Select(async i =>
        {
            var message = DiscordMessageBuilder.Create()
                .WithMessageId(i)
                .WithContent($"Message {i}")
                .Build();
            return await Client.PostAsJsonAsync("/api/messages", message);
        });

        await Task.WhenAll(tasks);

        var finalMemory = GC.GetTotalMemory(false);

        // Assert
        _measurements.Should().Contain(m =>
            m.MetricName == "system.memory.used" &&
            m.Value > 0);

        _measurements.Should().Contain(m =>
            m.MetricName == "system.active.requests" &&
            m.Value >= 0);
    }

    public override void Dispose()
    {
        _meterProvider?.Dispose();
        base.Dispose();
    }
}
