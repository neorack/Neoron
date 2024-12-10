using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Interfaces;
using Neoron.API.Tests.Fixtures;
using FluentAssertions;

namespace Neoron.API.Tests.Integration
{
    [Collection("Database")]
    public class UserTokenIntegrationTests : IntegrationTestBase
    {
        private readonly IUserTokenRepository _repository;

        public UserTokenIntegrationTests(TestWebApplicationFactory<Program> factory) 
            : base(factory)
        {
            _repository = factory.Services.GetRequiredService<IUserTokenRepository>();
        }

        [Fact]
        public async Task TokenLifecycle_ShouldWorkAsExpected()
        {
            // Arrange
            const long userId = 12345;
            
            // Act - Create token
            var token = await _repository.CreateTokenAsync(userId);
            
            // Assert - Token created successfully
            token.Should().NotBeNull();
            token.UserId.Should().Be(userId);

            // Act - Validate token
            var validatedToken = await _repository.ValidateTokenAsync(token.Token);
            
            // Assert - Token is valid
            validatedToken.Should().NotBeNull();
            validatedToken!.Token.Should().Be(token.Token);

            // Act - Revoke token
            var revoked = await _repository.RevokeTokenAsync(token.Token);
            
            // Assert - Token was revoked
            revoked.Should().BeTrue();

            // Act - Try to validate revoked token
            var invalidToken = await _repository.ValidateTokenAsync(token.Token);
            
            // Assert - Revoked token is invalid
            invalidToken.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveTokens_ShouldReturnOnlyValidTokens()
        {
            // Arrange
            const long userId = 12345;
            var token1 = await _repository.CreateTokenAsync(userId);
            var token2 = await _repository.CreateTokenAsync(userId);
            await _repository.RevokeTokenAsync(token2.Token);
            
            // Act
            var activeTokens = await _repository.GetActiveTokensAsync(userId);
            
            // Assert
            activeTokens.Should().HaveCount(1);
            activeTokens.Single().Token.Should().Be(token1.Token);
        }
    }
}
