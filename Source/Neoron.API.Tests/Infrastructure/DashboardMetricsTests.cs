using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Neoron.API.Tests.Infrastructure;

[Collection("Database")]
public class DashboardMetricsTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public DashboardMetricsTests(
        TestWebApplicationFactory<Program> factory,
        ITestOutputHelper output) : base(factory)
    {
        _output = output;
        Cleanup();
    }

    [Fact]
    public async Task Metrics_Endpoint_ReturnsPrometheusMetrics()
    {
        // Act
        var response = await Client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");
        
        // Verify required metrics are present
        content.Should().Contain("# HELP");
        content.Should().Contain("# TYPE");
        content.Should().Contain("messages_total");
        content.Should().Contain("message_processing_duration");
    }

    [Fact]
    public async Task Health_Endpoint_ReturnsHealthStatus()
    {
        // Act
        var response = await Client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var healthStatus = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        healthStatus.Should().ContainKey("status");
        healthStatus!["status"].ToString().Should().Be("Healthy");
    }

    [Fact]
    public async Task Dashboard_Metrics_IncludeRequiredData()
    {
        // Act
        var response = await Client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        // System metrics
        content.Should().Contain("process_cpu_seconds_total");
        content.Should().Contain("process_memory_usage");
        content.Should().Contain("dotnet_total_memory_bytes");

        // HTTP metrics
        content.Should().Contain("http_requests_total");
        content.Should().Contain("http_request_duration_seconds");

        // Database metrics
        content.Should().Contain("database_connections");
        content.Should().Contain("database_query_duration_seconds");

        // Application metrics
        content.Should().Contain("messages_processed_total");
        content.Should().Contain("cache_hit_ratio");
        content.Should().Contain("rate_limits_exceeded_total");
    }

    [Fact]
    public async Task Dashboard_Metrics_IncludeLabels()
    {
        // Act
        var response = await Client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        // Verify metric labels
        content.Should().Contain("endpoint=");
        content.Should().Contain("method=");
        content.Should().Contain("status_code=");
        content.Should().Contain("error_type=");
    }

    [Fact]
    public async Task Dashboard_Metrics_UpdateWithActivity()
    {
        // Arrange
        var initialMetrics = await Client.GetAsync("/metrics");
        var initialContent = await initialMetrics.Content.ReadAsStringAsync();

        // Generate some activity
        await Client.GetAsync("/api/messages/1");
        await Client.GetAsync("/api/messages/999"); // Will cause 404
        await Client.PostAsync("/api/messages", new StringContent("invalid")); // Will cause 400

        // Act
        var updatedMetrics = await Client.GetAsync("/metrics");
        var updatedContent = await updatedMetrics.Content.ReadAsStringAsync();

        // Assert
        updatedContent.Should().NotBe(initialContent);
        
        // Log the changes
        _output.WriteLine("Metrics changes detected:");
        var initialLines = initialContent.Split('\n');
        var updatedLines = updatedContent.Split('\n');
        var changes = updatedLines.Except(initialLines);
        
        foreach (var change in changes)
        {
            _output.WriteLine(change);
        }
    }

    [Fact]
    public async Task Dashboard_Metrics_IncludeHistograms()
    {
        // Act
        var response = await Client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        // Verify histogram metrics
        content.Should().Contain("_bucket{le=");
        content.Should().Contain("_sum");
        content.Should().Contain("_count");

        // Specific histograms
        content.Should().Contain("http_request_duration_seconds_bucket");
        content.Should().Contain("database_query_duration_seconds_bucket");
    }

    [Fact]
    public async Task Dashboard_Metrics_ReflectErrorStates()
    {
        // Arrange
        var initialMetrics = await Client.GetAsync("/metrics");
        var initialContent = await initialMetrics.Content.ReadAsStringAsync();

        // Generate some errors
        await Client.GetAsync("/api/messages/-1"); // Invalid ID
        await Client.GetAsync("/nonexistent-endpoint"); // 404
        await Client.PostAsync("/api/messages", new StringContent("{")); // Invalid JSON

        // Act
        var updatedMetrics = await Client.GetAsync("/metrics");
        var updatedContent = await updatedMetrics.Content.ReadAsStringAsync();

        // Assert
        updatedContent.Should().Contain("http_server_errors_total");
        updatedContent.Should().Contain("http_client_errors_total");
        updatedContent.Should().Contain("application_errors_total");
    }

    [Fact]
    public async Task Dashboard_Metrics_IncludeCustomMetrics()
    {
        // Act
        var response = await Client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        // Business metrics
        content.Should().Contain("messages_by_type");
        content.Should().Contain("messages_by_guild");
        content.Should().Contain("active_users");
        content.Should().Contain("message_processing_queue");
    }
}
