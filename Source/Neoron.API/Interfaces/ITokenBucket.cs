namespace Neoron.API.Interfaces
{
    /// <summary>
    /// Represents a token bucket rate limiter.
    /// </summary>
    public interface ITokenBucket : IDisposable
    {
        /// <summary>
        /// Attempts to consume a token from the bucket.
        /// </summary>
        /// <returns>True if a token was successfully consumed, false if no tokens are available.</returns>
        bool ConsumeToken();
    }
}
