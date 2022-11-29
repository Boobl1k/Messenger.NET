using System;
using EventBusHandler.Context;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Domain;
using Domain.File;
using EventBusHandler.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace EventBusHandler.Services;

public class BusHandlerService : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(1));
    private readonly ILogger<BusHandlerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly BucketsOptions _bucketsOptions;
    private readonly MongoOptions _mongoOptions;
    private readonly RabbitOptions _rabbitOptions;

    private IConnection? _messagesConnection;
    private IConnection? _fileMetasConnection;
    private readonly AmazonS3Client _s3Client;

    public BusHandlerService(ILogger<BusHandlerService> logger, IServiceProvider serviceProvider,
        IOptions<BucketsOptions> bucketOptions, IOptions<S3Options> s3Options,
        IOptions<MongoOptions> mongoOptions, IOptions<RabbitOptions> rabbitOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _mongoOptions = mongoOptions.Value;
        _bucketsOptions = bucketOptions.Value;
        _rabbitOptions = rabbitOptions.Value;

        _s3Client = new AmazonS3Client(s3Options.Value.KeyId, s3Options.Value.SecretKey,
            new AmazonS3Config { ServiceURL = s3Options.Value.ConnectionString, ForcePathStyle = true });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            await RemakeAsync();

            if (await _timer.WaitForNextTickAsync(stoppingToken))
                KillConnections();
        } while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }

    private async Task RemakeAsync()
    {
        IConnectionMultiplexer redisConnection = await ConnectionMultiplexer.ConnectAsync("redis");

        var mongoClient = new MongoClient(_mongoOptions.ConnectionString);
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitOptions.HostName,
                DispatchConsumersAsync = true
            };
            {
                _messagesConnection = factory.CreateConnection();
                var channel = _messagesConnection.CreateModel();
                channel.QueueDeclare(queue: _rabbitOptions.MessagesQueue, durable: true, exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation("{Now}: Messages connection opened", DateTime.Now);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (_, ea) =>
                {
                    var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<Message>(content);
                    if (message is null)
                    {
                        _logger.LogInformation("Broken message: \'{Content}\'", content);
                        return;
                    }

                    _logger.LogInformation("New message: {MessageUserName} says \'{MessageText}\'", message.UserName,
                        message.Text);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        context.Messages.Add(message);
                        await context.SaveChangesAsync();
                    }

                    channel.BasicAck(ea.DeliveryTag, false);
                    await Task.Yield();
                };
                channel.BasicConsume(_rabbitOptions.MessagesQueue, false, consumer);
                //maybe needed waiter? Task.Sleep() or latch.WaitOne(2000); according to rabbit docs*
            }

            {
                _fileMetasConnection = factory.CreateConnection();
                var channel = _fileMetasConnection.CreateModel();
                channel.QueueDeclare(queue: _rabbitOptions.FileMetasQueue, durable: true, exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation("{Now}: File metas connection opened", DateTime.Now);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (_, ea) =>
                {
                    try
                    {
                        var fileId = Encoding.UTF8.GetString(ea.Body.ToArray());

                        Console.WriteLine(fileId);

                        if (!(await _s3Client.ListBucketsAsync()).Buckets.Exists(b =>
                                b.BucketName == _bucketsOptions.PermBucketName))
                            await _s3Client.PutBucketAsync(_bucketsOptions.PermBucketName);

                        await _s3Client.CopyObjectAsync(_bucketsOptions.TempBucketName, fileId,
                            _bucketsOptions.PermBucketName,
                            fileId);

                        var db = redisConnection.GetDatabase();
                        var data = db.StringGet(fileId);
                        if (!data.HasValue) throw new Exception();
                        Console.WriteLine(data);
                        try
                        {
                            var meta = JsonSerializer.Deserialize<SoundFileMeta>(data!) ?? throw new Exception();
                            if (meta is not { Album: { }, Author: { } })
                                throw new Exception();
                            await mongoClient.GetDatabase(_mongoOptions.DatabaseName)
                                .GetCollection<SoundFileMeta>(_mongoOptions.SoundCollectionName)
                                .InsertOneAsync(meta);
                        }
                        catch
                        {
                            try
                            {
                                var meta = JsonSerializer.Deserialize<VideoFileMeta>(data!) ?? throw new Exception();
                                if (meta is not { Producer: { }, Studio: { } })
                                    throw new Exception();
                                await mongoClient.GetDatabase(_mongoOptions.DatabaseName)
                                    .GetCollection<VideoFileMeta>(_mongoOptions.VideoCollectionName)
                                    .InsertOneAsync(meta);
                            }
                            catch
                            {
                                var meta = JsonSerializer.Deserialize<TextFileMeta>(data!) ?? throw new Exception();
                                await mongoClient.GetDatabase(_mongoOptions.DatabaseName)
                                    .GetCollection<TextFileMeta>(_mongoOptions.TextCollectionName)
                                    .InsertOneAsync(meta);
                            }
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                        await Task.Yield();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                };
                channel.BasicConsume(_rabbitOptions.FileMetasQueue, false, consumer);
                //maybe waiter is needed? Task.Sleep() or latch.WaitOne(2000); according to rabbit docs*
            }
        }
        catch (Exception e)
        {
            _logger.LogError("ERROR: \'{ErrorMessage}\'", e.Message);
        }
    }

    public override void Dispose()
    {
        KillConnections();
        base.Dispose();
    }

    private void KillConnections()
    {
        _logger.LogWarning("{Now}: Closing connections", DateTime.Now);

        _messagesConnection?.Close();
        _fileMetasConnection?.Close();
        _s3Client.Dispose();
    }
}