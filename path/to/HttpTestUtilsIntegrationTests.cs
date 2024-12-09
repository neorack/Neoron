using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;

[TestFixture]
public class HttpTestUtilsIntegrationTests
{
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        // Initialize HttpClient or test server
        _client = new HttpClient();
    }

    [Test]
    public async Task DeleteAsync_ShouldSendDeleteRequest()
    {
        // Arrange
        var url = "http://example.com/resource";

        // Act
        var response = await HttpTestUtils.DeleteAsync(_client, url);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        // Add more assertions to verify response handling
    }
}
