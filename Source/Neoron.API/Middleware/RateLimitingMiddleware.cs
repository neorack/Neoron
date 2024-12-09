using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Neoron.API.Middleware
{
    public class RateLimitingOptions
    {
        public int MaxTokens { get; set; } = 100;
        public int RefillRate { get; set; } = 10;
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);
    }

    public class RateLimitingMiddleware : IDisposable
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RateLimitingOptions _options;
        private readonly Timer _cleanupTimer;
        private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IOptions<RateLimitingOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            
            _cleanupTimer = new Timer(CleanupBuckets, null, _options.CleanupInterval, _options.CleanupInterval);
        }

        private void CleanupBuckets(object? state)
        {
            var now = DateTime.UtcNow;
            var staleKeys = _buckets
                .Where(kvp => (now - kvp.Value.LastUsed).TotalHours > 1)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in staleKeys)
            {
                _buckets.TryRemove(key, out _);
            }
        }

        public void Dispose() => _cleanupTimer?.Dispose();

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var key = GetClientKey(context);
            var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_options.MaxTokens, _options.RefillRate));

            if (!bucket.TryTake())
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientKey}", key);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new { error = "Too many requests" }).ConfigureAwait(false);
                return;
            }

            await _next(context).ConfigureAwait(false);
        }

        private static string GetClientKey(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var apiKey = context.Request.Headers["X-API-Key"].ToString();
            var path = context.Request.Path.ToString();
            
            // Include API endpoint in the key to allow different rates for different endpoints
            return string.IsNullOrEmpty(apiKey) 
                ? $"{clientIp}:{path}"
                : $"{clientIp}:{apiKey}:{path}";
        }
    }

    public class TokenBucket(int maxTokens = 100, int refillRate = 10)
    {
        private readonly int _maxTokens = maxTokens;
        private readonly int _refillRate = refillRate;
        private double _tokens = maxTokens;
        private DateTime _lastRefill = DateTime.UtcNow;
        private readonly object _lock = new();
        public DateTime LastUsed { get; private set; } = DateTime.UtcNow;

        public bool TryTake()
        {
            lock (_lock)
            {
                RefillTokens();
                LastUsed = DateTime.UtcNow;

                if (_tokens < 1)
                {
                    return false;
                }

                _tokens--;
                return true;
            }
        }

        private void RefillTokens()
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - _lastRefill).TotalSeconds;
            var tokensToAdd = elapsed * _refillRate;

            _tokens = Math.Min(_maxTokens, _tokens + tokensToAdd);
            _lastRefill = now;
        }
    }
}
