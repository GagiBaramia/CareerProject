using CareerProject.RecommendationService.Config;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CareerProject.RecommendationService.Caching;

public class RedisAiChatRateLimiter(
    IConnectionMultiplexer redis,
    IOptions<AiChatOptions> options,
    ILogger<RedisAiChatRateLimiter> logger) : IAiChatRateLimiter
{
    private const string KeyPrefix = "aichat:ratelimit:";

    private readonly AiChatRateLimitOptions _options = options.Value.RateLimit;

    // Fixed-window counter. If Redis is unreachable, the request is allowed through -
    // an infra hiccup on this safeguard should not block a legitimate AI chat request.
    public async Task<bool> TryAcquireAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var db = redis.GetDatabase();
            var key = $"{KeyPrefix}{userId}";

            var count = await db.StringIncrementAsync(key);
            if (count == 1)
            {
                await db.KeyExpireAsync(key, TimeSpan.FromSeconds(_options.WindowSeconds));
            }

            return count <= _options.MaxRequests;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI chat rate limit check failed for user {UserId}, allowing the request.", userId);
            return true;
        }
    }
}
