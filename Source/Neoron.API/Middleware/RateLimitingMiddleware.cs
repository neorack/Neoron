using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neoron.API.Extensions;
using Neoron.API.Models;

namespace Neoron.API.Middleware
{
    /// <summary>
    /// Middleware for rate limiting requests.
    /// </summary>
    public class RateLimitingMiddleware : IDisposable
    {

        private readonly RequestDelegate next;
        private readonly ILogger<RateLimitingMiddleware> logger;
        private readonly RateLimitingOptions options;
        private readonly ITokenBucket tokenBucket;
        private readonly Timer cleanupTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="options">The rate limiting options.</param>
        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IOptions<RateLimitingOptions> options)
        {
            ArgumentNullException.ThrowIfNull(next);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(options);

            this.next = next;
            this.logger = logger;
            this.options = options.Value;
            tokenBucket = new TokenBucket(options.MaxTokens, options.TokenRefillRate);

            // Start the cleanup timer
            cleanupTimer = new Timer(Cleanup, null, TimeSpan.Zero, options.Value.CleanupInterval);
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the middleware invocation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!tokenBucket.ConsumeToken())
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too Many Requests").ConfigureAwait(false);
                LoggingMessages.FailedToAddMessage(logger, context.Connection.RemoteIpAddress?.ToString(), new InvalidOperationException("Rate limit exceeded"));
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
            // Log cleanup activity
            logger.LogDebug("Performing periodic rate limiting cleanup");
            
            // Could implement additional cleanup logic here if needed
            // For example: clearing any cached data, updating metrics, etc.
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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
