using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Neoron.API.Tests.Fixtures;
using Xunit;

namespace Neoron.API.Tests.Security;

[Collection("Database")]
public class AuthenticationTests : IntegrationTestBase
{
    public AuthenticationTests(TestWebApplicationFactory<Program> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task SecuredEndpoint_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "valid-test-token");

        // Act
        var response = await Client.GetAsync("/api/messages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SecuredEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/messages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SecuredEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await Client.GetAsync("/api/messages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SecuredEndpoint_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "expired-token");

        // Act
        var response = await Client.GetAsync("/api/messages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
