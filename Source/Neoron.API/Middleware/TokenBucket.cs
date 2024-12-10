using System;
using System.Threading;

namespace Neoron.API.Middleware
{
    /// <summary>
    /// Represents a token bucket rate limiter.
    /// </summary>
    public class TokenBucket : IDisposable
    {
        private readonly int maxTokens;
        private readonly double refillRate;
        private readonly object lockObject = new object();
        private readonly Timer refillTimer;
        private int tokens;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenBucket"/> class.
        /// </summary>
        /// <param name="maxTokens">The maximum number of tokens.</param>
        /// <param name="refillRate">The rate at which tokens are refilled per second.</param>
        public TokenBucket(int maxTokens, double refillRate)
        {
            this.maxTokens = maxTokens;
            this.refillRate = refillRate;
            tokens = maxTokens;
            refillTimer = new Timer(RefillTokens, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Attempts to consume a token.
        /// </summary>
        /// <returns>True if a token was consumed, false otherwise.</returns>
        public bool ConsumeToken()
        {
            lock (lockObject)
            {
                if (tokens > 0)
                {
                    tokens--;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                refillTimer.Dispose();
            }
        }

        private void RefillTokens(object? state)
        {
            lock (lockObject)
            {
                tokens = Math.Min(tokens + (int)(refillRate * 1.0), maxTokens);
            }
        }
    }
}
