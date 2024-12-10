using System.Net.Http.Json;
using FluentAssertions;
using Neoron.API.Models;
using Neoron.API.Tests.Builders;
using Neoron.API.Tests.Fixtures;
using Neoron.API.Tests.Helpers;
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
}
