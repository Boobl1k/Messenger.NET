using EventBusHandler.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventBusHandler.Data.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _db;

    public MongoDbContext(IOptions<MongoOptions> configuration)
    {
        var mongoClient = new MongoClient(configuration.Value.ConnectionString);
        _db = mongoClient.GetDatabase(configuration.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) =>
        _db.GetCollection<T>(name);
}

public static class MongoExtensions
{
    public static void AddMongoDbContext(this IServiceCollection services) =>
        services.AddSingleton<MongoDbContext>();
}