using EventBusHandler.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EventBusHandler.Services;

public class RedisCacheService
{
    private readonly RedisOptions _redisOptions;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IOptions<RedisOptions> redisOptions, ILogger<RedisCacheService> logger,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
        _redisOptions = redisOptions.Value;
    }

    public async Task<RedisValue> GetAsync(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();
        return await db.StringGetAsync(key);
    }
}

public static class RedisExtensions
{
    public static void AddRedisCacheService(this IServiceCollection services) =>
        services.AddSingleton<RedisCacheService>();
}