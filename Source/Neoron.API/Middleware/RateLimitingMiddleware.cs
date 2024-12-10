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
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

        private readonly RequestDelegate next = default!;
        private readonly ILogger<RateLimitingMiddleware> logger = default!;
        private readonly RateLimitingOptions options = default!;
        private readonly TokenBucket tokenBucket;

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
            Timer cleanupTimer = new Timer(Cleanup, null, TimeSpan.Zero, CleanupInterval);
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
        /// Cleans up the middleware state.
        /// </summary>
        /// <param name="state">The state object.</param>
        protected void Cleanup(object? state)
        {
            // Perform any necessary cleanup
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
            }
        }

    }
}
