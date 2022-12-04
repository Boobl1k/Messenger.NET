using Domain.File;
using EventBusHandler.Data.Context;
using EventBusHandler.Options;
using Microsoft.Extensions.Options;

namespace EventBusHandler.Data.Repository;

public class TextFileMetaRepository : GenericRepository<TextFileMeta>
{
    public TextFileMetaRepository(MongoDbContext context, IOptions<MongoOptions> configuration) : base(context,
        configuration.Value.TextCollectionName)
    { }
}