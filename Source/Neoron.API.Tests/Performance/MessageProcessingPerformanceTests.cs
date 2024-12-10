using System.Diagnostics;
using FluentAssertions;
using Neoron.API.Tests.Builders;
using Neoron.API.Tests.Helpers;
using Neoron.API.Tests.Infrastructure;
using Xunit;

namespace Neoron.API.Tests.Performance;

[Trait("Category", TestCategories.Performance)]
public class MessageProcessingPerformanceTests : IntegrationTestBase
{
    [Fact]
    public async Task BulkMessageProcessing_Performance()
    {
        // Arrange
        var messages = Enumerable.Range(0, 1000)
            .Select(_ => new DiscordMessageBuilder().Build())
            .ToList();

        // Act
        var sw = Stopwatch.StartNew();
        await TestUtils.TestDataSeeder.SeedTestMessages(DbContext, messages.Count);
        sw.Stop();

        // Assert
        sw.ElapsedMilliseconds.Should().BeLessThan(5000); // 5 seconds max
    }
}
