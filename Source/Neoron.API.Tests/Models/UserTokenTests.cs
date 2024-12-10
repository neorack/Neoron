using Neoron.API.Models;
using FluentAssertions;

namespace Neoron.API.Tests.Models
{
    public class UserTokenTests
    {
        [Fact]
        public void UserToken_ShouldRequireUserId()
        {
            // Arrange & Act
            var action = () => new UserToken
            {
                Token = "test-token"
            };

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void UserToken_ShouldRequireToken()
        {
            // Arrange & Act
            var action = () => new UserToken
            {
                UserId = 123
            };

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void UserToken_ShouldCreateValidInstance()
        {
            // Arrange & Act
            var token = new UserToken
            {
                UserId = 123,
                Token = "test-token",
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
            };

            // Assert
            token.UserId.Should().Be(123);
            token.Token.Should().Be("test-token");
            token.IsRevoked.Should().BeFalse();
            token.LastUsedAt.Should().BeNull();
        }
    }
}
