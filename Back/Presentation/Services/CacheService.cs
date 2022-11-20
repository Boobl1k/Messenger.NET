using System.Text.Json;
using StackExchange.Redis;

namespace Presentation.Services;

public class CacheService
{
    private readonly IConnectionMultiplexer _connection;

    public CacheService(IConnectionMultiplexer connection) => _connection = connection;

    public async Task<T> GetValueAsync<T>(string id) where T : class
    {
        var db = _connection.GetDatabase();
        var data = await db.StringGetAsync(id);
        if (!data.HasValue) throw new Exception();
        return JsonSerializer.Deserialize<T>(data!) ?? throw new Exception();
    }

    public async Task SetValueAsync<T>(string id, T value) where T : class
    {
        var db = _connection.GetDatabase();
        await db.StringSetAsync(id, JsonSerializer.Serialize(value));
    }

    public async Task<T> GetValueAsync<T>(Guid id) where T : class => await this.GetValueAsync<T>(id.ToString());

    public async Task SetValueAsync<T>(Guid id, T value) where T : class =>
        await this.SetValueAsync(id.ToString(), value);
}