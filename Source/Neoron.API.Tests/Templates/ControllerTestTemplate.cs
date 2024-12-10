using Xunit;
using FluentAssertions;
using System.Net.Http;
using System.Threading.Tasks;

namespace Neoron.API.Tests.Templates;

[Collection("Integration")]
[Trait("Category", TestCategories.Controllers)]
public class ControllerTestTemplate : TestBase
{
    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Endpoint_ScenarioDescription_ExpectedBehavior(string method)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(method), "/api/endpoint");
        
        // Act
        var response = await Client.SendAsync(request);
        
        // Assert
        response.Should().BeSuccessful();
    }
}
