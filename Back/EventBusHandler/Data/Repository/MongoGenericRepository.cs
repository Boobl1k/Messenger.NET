using EventBusHandler.Data.Context;
using MongoDB.Driver;

namespace EventBusHandler.Data.Repository;

public class GenericRepository<TEntity>
{
    private readonly IMongoCollection<TEntity> _dbCollection;

    protected GenericRepository(MongoDbContext context, string collectionName) =>
        _dbCollection = context.GetCollection<TEntity>(collectionName);

    public async Task CreateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(typeof(TEntity).Name + " entity is null");

        await _dbCollection.InsertOneAsync(entity);
    }
}