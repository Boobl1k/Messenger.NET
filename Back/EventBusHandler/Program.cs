using Amazon.S3;
using EventBusHandler.Data;
using EventBusHandler.Data.Context;
using EventBusHandler.Options;
using EventBusHandler.Services;
using EventBusHandler.Services.Consumer;
using RabbitMQ.Client;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services
    .Configure<BucketsOptions>(
        builder.Configuration.GetSection(BucketsOptions.Buckets))
    .Configure<S3Options>(
        builder.Configuration.GetSection(S3Options.S3Configuration))
    .Configure<MongoOptions>(
        builder.Configuration.GetSection(MongoOptions.MongoConfiguration))
    .Configure<RabbitOptions>(
        builder.Configuration.GetSection(RabbitOptions.RabbitConfiguration))
    .Configure<RedisOptions>(
        builder.Configuration.GetSection(RedisOptions.RedisConfiguration));

services.AddDbContext<AppDbContext>();
services.AddMongoDbContext();
services.AddUnitOfWork();
services.AddRedisCacheService();

var redisOptions = builder.Configuration.GetSection(RedisOptions.RedisConfiguration).Get<RedisOptions>();
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions.ConnectionString));

var s3Options = builder.Configuration.GetSection(S3Options.S3Configuration).Get<S3Options>();
services.AddSingleton(new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey,
    new AmazonS3Config { ServiceURL = s3Options.ConnectionString, ForcePathStyle = true }));

var rabbitOptions = builder.Configuration.GetSection(RabbitOptions.RabbitConfiguration).Get<RabbitOptions>();
services.AddSingleton(new ConnectionFactory
{
    HostName = rabbitOptions.HostName,
    DispatchConsumersAsync = true
});

services
    .AddSingleton<MessageConsumer>()
    .AddSingleton<FileMetaConsumer>();

services
    .AddHostedService<FileMetaBusHandlerService>()
    .AddHostedService<MessageBusHandlerService>();

builder.Build().Run();