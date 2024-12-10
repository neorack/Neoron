using Neoron.API.Models;

namespace Neoron.API.Interfaces
{
    /// <summary>
    /// Repository interface for managing user tokens.
    /// </summary>
    public interface IUserTokenRepository
    {
        /// <summary>
        /// Creates a new token for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="expiresIn">Optional token lifetime.</param>
        /// <returns>The created token.</returns>
        Task<UserToken> CreateTokenAsync(long userId, TimeSpan? expiresIn = null);

        /// <summary>
        /// Validates a token.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>The token if valid, null if invalid.</returns>
        Task<UserToken?> ValidateTokenAsync(string token);

        /// <summary>
        /// Revokes a token.
        /// </summary>
        /// <param name="token">The token to revoke.</param>
        /// <returns>True if revoked, false if not found.</returns>
        Task<bool> RevokeTokenAsync(string token);

        /// <summary>
        /// Gets active tokens for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>Collection of active tokens.</returns>
        Task<IEnumerable<UserToken>> GetActiveTokensAsync(long userId);
    }
}
