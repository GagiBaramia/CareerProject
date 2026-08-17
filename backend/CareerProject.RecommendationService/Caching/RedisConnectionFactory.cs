using StackExchange.Redis;

namespace CareerProject.RecommendationService.Caching;

// Builds the Redis connection string from environment variables only - mirrors the
// REDIS_* pattern already used for Postgres/RabbitMQ. abortConnect=false means the
// service still starts (and the caller-side try/catch still applies) even if Redis
// is unreachable at boot, instead of crashing the whole service.
public static class RedisConnectionFactory
{
    public static ConnectionMultiplexer Connect()
    {
        var host = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";

        // Short timeouts + no retry so a downed Redis fails fast (~1s) instead of the
        // library's 5s default - callers already catch and degrade gracefully, but only
        // if the failure itself doesn't stall the request.
        return ConnectionMultiplexer.Connect(
            $"{host}:{port},abortConnect=false,connectTimeout=1000,syncTimeout=1000,connectRetry=1");
    }
}
