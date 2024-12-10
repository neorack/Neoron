using Microsoft.EntityFrameworkCore;
using Neoron.API.Data;
using Neoron.API.Models;
using Neoron.API.Repositories;
using FluentAssertions;

namespace Neoron.API.Tests.Repositories
{
    public class UserTokenRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserTokenRepository _repository;

        public UserTokenRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserTokenRepository(_context);
        }

        [Fact]
        public async Task CreateTokenAsync_ShouldCreateValidToken()
        {
            // Arrange
            const long userId = 123;
            var expiresIn = TimeSpan.FromDays(1);

            // Act
            var token = await _repository.CreateTokenAsync(userId, expiresIn);

            // Assert
            token.Should().NotBeNull();
            token.UserId.Should().Be(userId);
            token.Token.Should().NotBeNullOrEmpty();
            token.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
            token.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.Add(expiresIn), TimeSpan.FromSeconds(1));
            token.IsRevoked.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ShouldReturnToken()
        {
            // Arrange
            var token = await _repository.CreateTokenAsync(123);
            
            // Act
            var result = await _repository.ValidateTokenAsync(token.Token);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().Be(token.Token);
            result.LastUsedAt.Should().NotBeNull();
            result.LastUsedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task ValidateTokenAsync_WithExpiredToken_ShouldReturnNull()
        {
            // Arrange
            var token = await _repository.CreateTokenAsync(123, TimeSpan.FromSeconds(-1));
            
            // Act
            var result = await _repository.ValidateTokenAsync(token.Token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithRevokedToken_ShouldReturnNull()
        {
            // Arrange
            var token = await _repository.CreateTokenAsync(123);
            await _repository.RevokeTokenAsync(token.Token);
            
            // Act
            var result = await _repository.ValidateTokenAsync(token.Token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RevokeTokenAsync_ShouldRevokeToken()
        {
            // Arrange
            var token = await _repository.CreateTokenAsync(123);
            
            // Act
            var result = await _repository.RevokeTokenAsync(token.Token);

            // Assert
            result.Should().BeTrue();
            var dbToken = await _context.UserTokens.FirstOrDefaultAsync(t => t.Token == token.Token);
            dbToken.Should().NotBeNull();
            dbToken!.IsRevoked.Should().BeTrue();
        }

        [Fact]
        public async Task GetActiveTokensAsync_ShouldReturnOnlyActiveTokens()
        {
            // Arrange
            const long userId = 123;
            await _repository.CreateTokenAsync(userId);
            var expiredToken = await _repository.CreateTokenAsync(userId, TimeSpan.FromSeconds(-1));
            var revokedToken = await _repository.CreateTokenAsync(userId);
            await _repository.RevokeTokenAsync(revokedToken.Token);

            // Act
            var activeTokens = await _repository.GetActiveTokensAsync(userId);

            // Assert
            activeTokens.Should().HaveCount(1);
            activeTokens.Should().NotContain(t => t.Token == expiredToken.Token);
            activeTokens.Should().NotContain(t => t.Token == revokedToken.Token);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
