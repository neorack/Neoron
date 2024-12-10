using Microsoft.Extensions.Options;
using Neoron.API.Models;

namespace Neoron.API.Middleware
{
    /// <summary>
    /// Middleware for rate limiting requests.
    /// </summary>
    public class RateLimitingMiddleware : IDisposable
    {
        private static readonly Action<ILogger, string, Exception?> LogRateLimitExceeded =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(1, "RateLimitExceeded"),
                "Rate limit exceeded for IP: {IpAddress}");

        private static readonly Action<ILogger, Exception?> LogCleanup =
            LoggerMessage.Define(
                LogLevel.Debug,
                new EventId(2, "Cleanup"),
                "Performing periodic rate limiting cleanup");

        private readonly RequestDelegate next;
        private readonly ILogger<RateLimitingMiddleware> logger;
        private readonly RateLimitingOptions options;
        private readonly TokenBucket tokenBucket;
        private readonly Timer cleanupTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="options">The rate limiting options.</param>
        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IOptions<RateLimitingOptions> options)
        {
            ArgumentNullException.ThrowIfNull(next);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(options);

            this.next = next;
            this.logger = logger;
            this.options = options.Value;
            tokenBucket = new TokenBucket(
                this.options.MaxTokens,
                this.options.TokenRefillRate,
                this.options.BurstSize);

            // Start the cleanup timer
            cleanupTimer = new Timer(Cleanup, null, TimeSpan.Zero, this.options.CleanupInterval);
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the middleware invocation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Add rate limit headers
            context.Response.Headers.Append(
                "X-RateLimit-Limit",
                options.MaxTokens.ToString(System.Globalization.CultureInfo.InvariantCulture));

            if (!tokenBucket.ConsumeToken())
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Append("Retry-After", "1");
                await context.Response.WriteAsync("Too Many Requests").ConfigureAwait(false);
                LogRateLimitExceeded(
                    logger,
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    null);
                return;
            }

            await next(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs periodic cleanup of middleware resources.
        /// </summary>
        /// <param name="state">The state object (unused).</param>
        protected void Cleanup(object? state)
        {
            LogCleanup(logger, null);

            // Could implement additional cleanup logic here if needed
            // For example: clearing any cached data, updating metrics, etc.
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                tokenBucket.Dispose();
                cleanupTimer.Dispose();
            }
        }
    }
}
