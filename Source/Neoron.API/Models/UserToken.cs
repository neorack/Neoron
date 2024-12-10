using System.ComponentModel.DataAnnotations;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a user's authentication token.
    /// </summary>
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
