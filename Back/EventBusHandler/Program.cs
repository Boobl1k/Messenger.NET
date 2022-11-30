using EventBusHandler.Data;
using EventBusHandler.Data.Context;
using EventBusHandler.Options;
using EventBusHandler.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services
    .Configure<BucketsOptions>(
        builder.Configuration.GetSection(BucketsOptions.Buckets))
    .Configure<S3Options>(
        builder.Configuration.GetSection(S3Options.S3))
    .Configure<MongoOptions>(
        builder.Configuration.GetSection(MongoOptions.MongoConfiguration))
    .Configure<RabbitOptions>(
        builder.Configuration.GetSection(RabbitOptions.RabbitConfiguration))
    .Configure<RedisOptions>(
        builder.Configuration.GetSection(RedisOptions.RedisConfiguration));

services.AddDbContext<AppDbContext>();
services.AddMongoDbContext();
services.AddUnitOfWork();

services.AddSingleton<IConnectionMultiplexer>(x =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetSection(RedisOptions.RedisConfiguration)
        .GetValue<string>(new RedisOptions().ConnectionString)));

var redisOptions = builder.Configuration.GetSection(RedisOptions.RedisConfiguration).Get<RedisOptions>();
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
services.AddRedisCacheService();

services
    .AddHostedService<FileMetaBusHandlerService>()
    .AddHostedService<MessageBusHandlerService>();

builder.Build().Run();