using System.Collections.Concurrent;

namespace Neoron.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GetClientKey(context);
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket());

        if (!bucket.TryTake())
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientKey}", key);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new { error = "Too many requests" });
            return;
        }

        await _next(context);
    }

    private static string GetClientKey(HttpContext context)
    {
        // Use IP address and optional API key for rate limiting
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        return $"{clientIp}:{apiKey}";
    }
}

public class TokenBucket
{
    private const int MaxTokens = 100;
    private const int RefillRate = 10; // tokens per second
    private double _tokens;
    private DateTime _lastRefill;
    private readonly object _lock = new();

    public TokenBucket()
    {
        _tokens = MaxTokens;
        _lastRefill = DateTime.UtcNow;
    }

    public bool TryTake()
    {
        lock (_lock)
        {
            RefillTokens();

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
        var tokensToAdd = elapsed * RefillRate;

        _tokens = Math.Min(MaxTokens, _tokens + tokensToAdd);
        _lastRefill = now;
    }
}
