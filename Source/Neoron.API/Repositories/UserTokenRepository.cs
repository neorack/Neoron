using Microsoft.EntityFrameworkCore;
using Neoron.API.Data;
using Neoron.API.Interfaces;
using Neoron.API.Models;

namespace Neoron.API.Repositories
{
    /// <summary>
    /// Repository for managing user tokens.
    /// </summary>
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTokenRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public UserTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<UserToken> CreateTokenAsync(long userId, TimeSpan? expiresIn = null)
        {
            var token = new UserToken
            {
                UserId = userId,
                Token = GenerateToken(),
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.Add(expiresIn ?? TimeSpan.FromDays(30))
            };

            _ = _context.UserTokens.Add(token);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return token;
        }

        /// <inheritdoc/>
        public async Task<UserToken?> ValidateTokenAsync(string token)
        {
            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked)
                .ConfigureAwait(false);

            if (userToken == null || userToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return null;
            }

            userToken.LastUsedAt = DateTimeOffset.UtcNow;
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return userToken;
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeTokenAsync(string token)
        {
            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.Token == token)
                .ConfigureAwait(false);

            if (userToken == null)
            {
                return false;
            }

            userToken.IsRevoked = true;
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserToken>> GetActiveTokensAsync(long userId)
        {
            return await _context.UserTokens
                .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTimeOffset.UtcNow)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        private static string GenerateToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }
    }
}
