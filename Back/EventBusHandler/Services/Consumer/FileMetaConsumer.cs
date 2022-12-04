using System.Text;
using System.Text.Json;
using Amazon.S3;
using Domain.File;
using EventBusHandler.Data;
using EventBusHandler.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBusHandler.Services.Consumer;

internal class FileMetaConsumer : ConsumerBase, IDisposable
{
    private readonly ILogger<FileMetaConsumer> _logger;
    private readonly BucketsOptions _bucketsOptions;
    private readonly RedisCacheService _redisCacheService;
    private readonly IConnection? _fileMetasConnection;
    private readonly IModel _fileMetasChannel;
    private readonly AmazonS3Client _s3Client;

    public FileMetaConsumer(IOptions<RabbitOptions> rabbitOptions, ILogger<FileMetaConsumer> logger,
        IServiceProvider serviceProvider, AmazonS3Client s3Client, IOptions<BucketsOptions> bucketsOptions,
        RedisCacheService redisCacheService, ConnectionFactory connectionFactory) : base(rabbitOptions, serviceProvider,
        connectionFactory)
    {
        _logger = logger;
        _s3Client = s3Client;
        _redisCacheService = redisCacheService;
        _bucketsOptions = bucketsOptions.Value;

        _fileMetasConnection = ConnectionFactory.CreateConnection();
        _fileMetasChannel = _fileMetasConnection.CreateModel();
    }

    public void StartConsumingFileMetas()
    {
        _fileMetasChannel.QueueDeclare(queue: RabbitOptions.FileMetasQueue, durable: true, exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("{Now}: File metas connection opened", DateTime.Now);

        var consumer = new AsyncEventingBasicConsumer(_fileMetasChannel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var fileId = Encoding.UTF8.GetString(ea.Body.ToArray());

                _logger.LogDebug("got new file id={FileId} from bus", fileId);

                if (!(await _s3Client.ListBucketsAsync()).Buckets.Exists(b =>
                        b.BucketName == _bucketsOptions.PermBucketName))
                    await _s3Client.PutBucketAsync(_bucketsOptions.PermBucketName);

                await _s3Client.CopyObjectAsync(_bucketsOptions.TempBucketName, fileId,
                    _bucketsOptions.PermBucketName,
                    fileId);

                var data = await _redisCacheService.GetValueAsync(fileId);

                if (data is null) throw new Exception("Data is invalid");
                _logger.LogInformation("data of file meta: {Data}", data);


                using var scope = ServiceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();


                var soundMeta = JsonSerializer.Deserialize<SoundFileMeta>(data);
                if (soundMeta is { Album: { }, Author: { } })
                {
                    await unitOfWork.SoundFileMetas.CreateAsync(soundMeta);
                    _logger.LogInformation("Added Sound");
                }
                else
                {
                    var textMeta = JsonSerializer.Deserialize<VideoFileMeta>(data);
                    if (textMeta is { Producer: { }, Studio: { } })
                    {
                        await unitOfWork.VideoFileMetas.CreateAsync(textMeta);
                        _logger.LogInformation("Added Video");
                    }
                    else
                    {
                        var meta = JsonSerializer.Deserialize<TextFileMeta>(data);
                        if (meta is not { Name: { } })
                            throw new Exception();
                        await unitOfWork.TextFileMetas.CreateAsync(meta);
                        _logger.LogInformation("Added Text");
                    }
                }

                _fileMetasChannel.BasicAck(ea.DeliveryTag, false);
                await Task.Yield();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "😭 error in FileMetaBusHandlerService");
            }
        };
        _fileMetasChannel.BasicConsume(RabbitOptions.FileMetasQueue, false, consumer);
    }

    public void Dispose()
    {
        _logger.LogWarning("{Now}: Closing connections", DateTime.Now);

        _s3Client.Dispose();
        _fileMetasChannel.Dispose();

        if (_fileMetasConnection?.IsOpen ?? false)
            _fileMetasConnection.Dispose();
    }
}