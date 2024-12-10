using System.ComponentModel.DataAnnotations;

namespace Neoron.API.Models
{
    /// <summary>
    /// Options for rate limiting.
    /// </summary>
    public class RateLimitingOptions : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the maximum number of tokens allowed in the bucket.
        /// </summary>
        /// <remarks>Must be greater than 0.</remarks>
        public int MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the token refill rate per second.
        /// </summary>
        /// <remarks>Must be greater than 0.</remarks>
        public double TokenRefillRate { get; set; }

        /// <summary>
        /// Gets or sets the maximum burst size allowed.
        /// </summary>
        /// <remarks>Must be greater than or equal to MaxTokens.</remarks>
        public int BurstSize { get; set; }

        /// <summary>
        /// Gets or sets the interval for cleanup operations.
        /// </summary>
        /// <remarks>Defaults to 1 hour if not specified.</remarks>
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Validates the options.
        /// </summary>
        /// <param name="validationContext">The validation context containing information about the validation operation.</param>
        /// <returns>An enumerable of validation results.</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MaxTokens <= 0)
            {
                yield return new ValidationResult(
                    "MaxTokens must be greater than 0",
                    new[] { nameof(MaxTokens) });
            }

            if (TokenRefillRate <= 0)
            {
                yield return new ValidationResult(
                    "TokenRefillRate must be greater than 0",
                    new[] { nameof(TokenRefillRate) });
            }

            if (BurstSize < MaxTokens)
            {
                yield return new ValidationResult(
                    "BurstSize must be greater than or equal to MaxTokens",
                    new[] { nameof(BurstSize) });
            }

            if (CleanupInterval <= TimeSpan.Zero)
            {
                yield return new ValidationResult(
                    "CleanupInterval must be greater than zero",
                    new[] { nameof(CleanupInterval) });
            }
        }
    }
}
