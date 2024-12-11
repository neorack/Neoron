/*
 * Entity representing user authentication tokens
 * Manages token lifecycle and validation
 * 
 * Key design decisions:
 * - Implements token expiration
 * - Tracks token usage
 * - Supports token revocation
 * - Records creation metadata
 * - Uses secure token generation
 */

using System.ComponentModel.DataAnnotations;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a user's authentication token with full lifecycle management.
    /// </summary>
    /// <remarks>
    /// This entity:
    /// - Manages token creation and expiration
    /// - Tracks token usage timestamps
    /// - Supports explicit token revocation
    /// - Records token metadata
    /// - Implements secure token patterns
    /// </remarks>
    public class UserToken
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public required long UserId { get; set; }

        /// <summary>
        /// Gets or sets the token value.
        /// </summary>
        [Required]
        public required string Token { get; set; }

        /// <summary>
        /// Gets or sets when this token was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when this token expires.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets when this token was last used.
        /// </summary>
        public DateTimeOffset? LastUsedAt { get; set; }

        /// <summary>
        /// Gets or sets whether this token is revoked.
        /// </summary>
        public bool IsRevoked { get; set; }
    }
}
