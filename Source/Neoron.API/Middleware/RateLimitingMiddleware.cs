using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Neoron.API.Middleware;

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
        _next = next;
        _logger = logger;
        _options = options.Value;
        
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

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GetClientKey(context);
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_options.MaxTokens, _options.RefillRate));

        if (!bucket.TryTake())
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientKey}", key);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new { error = "Too many requests" });
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

public class TokenBucket
{
    private readonly int _maxTokens;
    private readonly int _refillRate;
    private double _tokens;
    private DateTime _lastRefill;
    private readonly object _lock = new();
    public DateTime LastUsed { get; private set; }

    public TokenBucket(int maxTokens = 100, int refillRate = 10)
    {
        _maxTokens = maxTokens;
        _refillRate = refillRate;
        _tokens = maxTokens;
        _lastRefill = DateTime.UtcNow;
        LastUsed = DateTime.UtcNow;
    }

    public bool TryTake()
    {
        lock (_lock)
        {
            RefillTokens();
            LastUsed = DateTime.UtcNow;

            if (_tokens < 1)
                return false;

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
