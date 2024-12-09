using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neoron.API.Services;

namespace Neoron.API.Tests.Fixtures;

public static class MockServices
{
    public static void AddMockServices(IServiceCollection services)
    {
        // Add mock services with default behaviors
        services.AddScoped(_ =>
        {
            var mockMessageService = new Mock<IMessageService>();
            // Configure default behaviors here if needed
            return mockMessageService.Object;
        });

        services.AddScoped(_ =>
        {
            var mockValidationService = new Mock<IValidationService>();
            // Configure default success validation
            mockValidationService
                .Setup(x => x.ValidateMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult { IsValid = true });
            return mockValidationService.Object;
        });

        // Add more mock services as needed
    }
}
