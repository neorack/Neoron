namespace Neoron.API.Models
{
    /// <summary>
    /// Options for rate limiting.
    /// </summary>
    public class RateLimitingOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of tokens.
        /// </summary>
        public int MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the token refill rate.
        /// </summary>
        public double TokenRefillRate { get; set; }
    }
}
