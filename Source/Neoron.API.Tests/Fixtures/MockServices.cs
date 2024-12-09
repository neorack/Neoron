using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neoron.API.Services;
using Neoron.API.Interfaces;
using Neoron.API.Models;
using Neoron.API.Validation;

namespace Neoron.API.Tests.Fixtures;

public static class MockServices
{
    public static void AddMockServices(IServiceCollection services)
    {
        // Message Service Mock
        services.AddScoped(_ =>
        {
            var mockMessageService = new Mock<IMessageService>();
            
            mockMessageService
                .Setup(x => x.GetMessageAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((long id, CancellationToken _) => new DiscordMessage 
                { 
                    MessageId = id,
                    Content = "Test Message",
                    CreatedAt = DateTimeOffset.UtcNow 
                });

            mockMessageService
                .Setup(x => x.CreateMessageAsync(It.IsAny<DiscordMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DiscordMessage message, CancellationToken _) => message);

            mockMessageService
                .Setup(x => x.UpdateMessageAsync(It.IsAny<DiscordMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DiscordMessage message, CancellationToken _) => message);

            mockMessageService
                .Setup(x => x.DeleteMessageAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return mockMessageService.Object;
        });

        // Validation Service Mock
        services.AddScoped(_ =>
        {
            var mockValidationService = new Mock<IValidationService>();
            
            mockValidationService
                .Setup(x => x.ValidateMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string content, CancellationToken _) => 
                    string.IsNullOrEmpty(content) 
                        ? MessageValidationResult.Error("Content cannot be empty")
                        : MessageValidationResult.Success());

            mockValidationService
                .Setup(x => x.ValidateMessageTypeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int type, CancellationToken _) => 
                    type < 0 || type > 10
                        ? MessageValidationResult.Error("Invalid message type")
                        : MessageValidationResult.Success());

            return mockValidationService.Object;
        });

        // Authorization Service Mock
        services.AddScoped(_ =>
        {
            var mockAuthService = new Mock<IAuthorizationService>();
            
            mockAuthService
                .Setup(x => x.CanModifyMessageAsync(
                    It.IsAny<long>(), 
                    It.IsAny<long>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((long messageAuthorId, long userId, CancellationToken _) => 
                    messageAuthorId == userId);

            mockAuthService
                .Setup(x => x.HasGuildPermissionAsync(
                    It.IsAny<long>(), 
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            return mockAuthService.Object;
        });

        // Rate Limiting Service Mock
        services.AddScoped(_ =>
        {
            var mockRateLimitService = new Mock<IRateLimitService>();
            
            mockRateLimitService
                .Setup(x => x.CheckRateLimitAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            return mockRateLimitService.Object;
        });
    }
}
