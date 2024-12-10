namespace Neoron.API.Models
{
    /// <summary>
    /// Options for rate limiting.
    /// </summary>
    public class RateLimitingOptions
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
        /// Gets or sets the interval for cleanup operations.
        /// </summary>
        /// <remarks>Defaults to 1 hour if not specified.</remarks>
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);
    }
}
