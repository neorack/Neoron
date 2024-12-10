namespace Neoron.API.Middleware
{
    /// <summary>
    /// Implements a token bucket rate limiter.
    /// </summary>
    public class TokenBucket : ITokenBucket
    {
        private readonly double maxTokens;
        private readonly double refillRate;
        private readonly double burstSize;
        private readonly object lockObject = new();
        private readonly Timer refillTimer;
        private readonly ILogger<TokenBucket> logger;
        private double tokens;
        private DateTime lastRefillTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenBucket"/> class.
        /// </summary>
        /// <param name="maxTokens">The maximum number of tokens.</param>
        /// <param name="refillRate">The rate at which tokens are refilled per second.</param>
        /// <param name="burstSize">The maximum number of tokens that can be accumulated.</param>
        public TokenBucket(int maxTokens, double refillRate, int burstSize, ILogger<TokenBucket> logger)
        {
            if (maxTokens <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxTokens), "Max tokens must be greater than 0");
            }

            if (refillRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(refillRate), "Refill rate must be greater than 0");
            }

            if (burstSize < maxTokens)
            {
                throw new ArgumentOutOfRangeException(nameof(burstSize), "Burst size must be greater than or equal to max tokens");
            }

            this.maxTokens = maxTokens;
            this.refillRate = refillRate;
            this.burstSize = burstSize;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            tokens = maxTokens;
            lastRefillTime = DateTime.UtcNow;
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
                int retryCount = 0;
                const int maxRetries = 3;
                const int retryDelayMs = 50;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        if (tokens > 0)
                        {
                            tokens--;
                            return true;
                        }

                        // If we're out of tokens, no point retrying immediately
                        return false;
                    }
                    catch (Exception ex) when (retryCount < maxRetries - 1)
                    {
                        retryCount++;
                        // Exponential backoff with jitter
                        var delay = retryDelayMs * Math.Pow(2, retryCount - 1);
                        var jitter = Random.Shared.Next(0, 50); // Add 0-50ms random jitter
                        Thread.Sleep((int)delay + jitter);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        // Log error on final retry before failing open
                        logger.LogError(ex, "Final retry attempt failed in TokenBucket");
                        return true;
                    }
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
                var now = DateTime.UtcNow;
                var timePassed = (now - lastRefillTime).TotalSeconds;
                var tokensToAdd = timePassed * refillRate;

                tokens = Math.Min(tokens + tokensToAdd, burstSize);
                lastRefillTime = now;
            }
        }
    }
}
