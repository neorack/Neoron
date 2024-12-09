using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Tests.Fixtures;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Xunit;
using Xunit.Abstractions;

namespace Neoron.API.Tests.Infrastructure;

[Collection("Database")]
public class AlertConfigurationTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly MeterProvider _meterProvider;
    private readonly List<Measurement<long>> _measurements;

    public AlertConfigurationTests(
        TestWebApplicationFactory<Program> factory,
        ITestOutputHelper output) : base(factory)
    {
        _output = output;
        _measurements = new List<Measurement<long>>();

        // Configure metrics collection for alerts
        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("Neoron.API.Alerts")
            .AddInMemoryExporter(_measurements)
            .Build()!;

        Cleanup();
    }

    [Fact]
    public async Task HighErrorRate_TriggersAlert()
    {
        // Arrange
        const int errorCount = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Generate errors
        for (int i = 0; i < errorCount; i++)
        {
            tasks.Add(Client.GetAsync("/api/messages/-1")); // Invalid ID
        }
        await Task.WhenAll(tasks);

        // Assert
        _measurements.Should().Contain(m =>
            m.MetricName == "error_rate_alert" &&
            m.Value >= 1);

        var alertMetric = _measurements.First(m => m.MetricName == "error_rate_alert");
        _output.WriteLine($"Alert metric value: {alertMetric.Value}");
        foreach (var tag in alertMetric.Tags)
        {
            _output.WriteLine($"Alert tag: {tag.Key} = {tag.Value}");
        }
    }

    [Fact]
    public async Task HighLatency_TriggersAlert()
    {
        // Arrange
        const int requestCount = 5;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Generate high latency requests
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(Client.GetAsync($"/api/messages/channel/1?take=1000")); // Large result set
        }
        await Task.WhenAll(tasks);

        // Assert
        _measurements.Should().Contain(m =>
            m.MetricName == "latency_alert" &&
            m.Tags.Contains(new KeyValuePair<string, string>("severity", "warning")));
    }

    [Fact]
    public async Task DatabaseConnections_TriggersAlert()
    {
        // Arrange
        const int concurrentRequests = 20;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Generate concurrent database requests
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(Client.GetAsync("/api/messages/1"));
        }
        await Task.WhenAll(tasks);

        // Assert
        _measurements.Should().Contain(m =>
            m.MetricName == "database_connections_alert" &&
            m.Value > 0);
    }

    [Fact]
    public async Task RateLimit_TriggersAlert()
    {
        // Arrange
        const int requestCount = 30;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Generate rate limit violations
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(Client.GetAsync("/api/messages/1"));
        }
        await Task.WhenAll(tasks);

        // Assert
        _measurements.Should().Contain(m =>
            m.MetricName == "rate_limit_alert" &&
            m.Value > 0);
    }

    [Fact]
    public async Task MemoryUsage_TriggersAlert()
    {
        // Arrange
        var largeDataRequests = new List<Task<HttpResponseMessage>>();
        const int requestCount = 10;

        // Act - Generate memory pressure
        for (int i = 0; i < requestCount; i++)
        {
            largeDataRequests.Add(Client.GetAsync($"/api/messages/channel/1?take=1000"));
        }
        await Task.WhenAll(largeDataRequests);

        // Assert
        _measurements.Should().Contain(m =>
            m.MetricName == "memory_usage_alert" &&
            m.Tags.Contains(new KeyValuePair<string, string>("type", "warning")));
    }

    [Fact]
    public async Task CacheHitRate_TriggersAlert()
    {
        // Arrange
        const int requestCount = 20;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Generate low cache hit rate
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(Client.GetAsync($"/api/messages/{i}")); // Different IDs to avoid cache hits
        }
        await Task.WhenAll(tasks);

        // Assert
        _measurements.Should().Contain(m =>
            m.MetricName == "cache_hit_rate_alert" &&
            m.Value < 0.5); // Less than 50% cache hit rate
    }

    [Fact]
    public async Task MessageProcessingQueue_TriggersAlert()
    {
        // Arrange
        const int messageCount = 25;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Generate message processing queue
        for (int i = 0; i < messageCount; i++)
        {
            var content = new StringContent($"{{\"content\": \"Test message {i}\"}}");
            tasks.Add(Client.PostAsync("/api/messages", content));
        }
        await Task.WhenAll(tasks);

        // Assert
        _measurements.Should().Contain(m =>
            m.MetricName == "message_queue_alert" &&
            m.Value > 0);
    }

    [Fact]
    public async Task AlertThresholds_AreConfigured()
    {
        // Act
        var response = await Client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().Contain("alert_threshold");
        content.Should().Contain("error_rate_threshold");
        content.Should().Contain("latency_threshold");
        content.Should().Contain("connection_threshold");
    }

    [Fact]
    public async Task AlertHistory_IsTracked()
    {
        // Arrange
        const int errorCount = 5;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Generate some alerts
        for (int i = 0; i < errorCount; i++)
        {
            tasks.Add(Client.GetAsync("/api/messages/-1")); // Invalid ID
        }
        await Task.WhenAll(tasks);

        // Assert
        _measurements.Should().Contain(m =>
            m.MetricName == "alert_history" &&
            m.Tags.Contains(new KeyValuePair<string, string>("type", "error_rate")));
    }

    public override void Dispose()
    {
        _meterProvider?.Dispose();
        base.Dispose();
    }
}
