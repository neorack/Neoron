using Xunit;
using Moq;
using FluentAssertions;
using System.Threading.Tasks;

namespace Neoron.API.Tests.Templates;

[Trait("Category", TestCategories.Services)]
public class ServiceTestTemplate
{
    private readonly Mock<IRepository> _repositoryMock;
    private readonly IService _service;
    
    public ServiceTestTemplate()
    {
        _repositoryMock = new Mock<IRepository>();
        _service = new Service(_repositoryMock.Object);
    }
    
    [Fact]
    public async Task Method_Scenario_ExpectedBehavior()
    {
        // Arrange
        _repositoryMock.Setup(/* ... */);
        
        // Act
        var result = await _service.Method();
        
        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(/* ... */);
    }
}
