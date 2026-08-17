using System.Text.Json;
using CareerProject.RecommendationService.Config;
using CareerProject.RecommendationService.Dtos;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CareerProject.RecommendationService.Caching;

public class RedisRecommendationCache(
    IConnectionMultiplexer redis,
    IOptions<RecommendationOptions> options,
    ILogger<RedisRecommendationCache> logger) : IRecommendationCache
{
    private const string KeyPrefix = "recommendations:";

    private readonly RecommendationOptions _options = options.Value;

    public async Task<List<JobRecommendationResponse>?> GetAsync(Guid personProfileId, CancellationToken cancellationToken)
    {
        try
        {
            var value = await redis.GetDatabase().StringGetAsync(Key(personProfileId));
            return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<List<JobRecommendationResponse>>(value!);
        }
        catch (Exception ex)
        {
            // Redis is a cache, not a source of truth - any failure here is just a cache miss.
            logger.LogWarning(ex, "Failed to read recommendation cache for PersonProfile {PersonProfileId}.", personProfileId);
            return null;
        }
    }

    public async Task SetAsync(Guid personProfileId, List<JobRecommendationResponse> jobs, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(jobs);
            await redis.GetDatabase().StringSetAsync(Key(personProfileId), json, TimeSpan.FromSeconds(_options.CacheTtlSeconds));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write recommendation cache for PersonProfile {PersonProfileId}.", personProfileId);
        }
    }

    public async Task InvalidateAsync(Guid personProfileId, CancellationToken cancellationToken)
    {
        try
        {
            await redis.GetDatabase().KeyDeleteAsync(Key(personProfileId));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to invalidate recommendation cache for PersonProfile {PersonProfileId}.", personProfileId);
        }
    }

    // A job change affects every person's recommendation list (the endpoint scores
    // every job for the requesting person), so every cached entry is invalidated.
    public async Task InvalidateAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var db = redis.GetDatabase();
            var server = redis.GetServer(redis.GetEndPoints().First());

            await foreach (var key in server.KeysAsync(pattern: $"{KeyPrefix}*"))
            {
                await db.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to invalidate recommendation cache after a job change.");
        }
    }

    private static string Key(Guid personProfileId) => $"{KeyPrefix}{personProfileId}";
}
