using StackExchange.Redis;

namespace EventBusHandler.Services;

public class RedisCacheService
{
    private readonly IConnectionMultiplexer _connection;

    public RedisCacheService(IConnectionMultiplexer connection) =>
        _connection = connection;

    public async Task<RedisValue> GetValueAsync(string key)
    {
        var db = _connection.GetDatabase();
        return await db.StringGetAsync(key);
    }
}

public static class RedisExtensions
{
    public static void AddRedisCacheService(this IServiceCollection services) =>
        services.AddSingleton<RedisCacheService>();
}