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
        RedisCacheService redisCacheService, ConnectionFactory connectionFactory) : base(rabbitOptions, serviceProvider, connectionFactory)
    {
        _logger = logger;
        _s3Client = s3Client;
        _redisCacheService = redisCacheService;
        _bucketsOptions = bucketsOptions.Value;
        
        _fileMetasConnection = ConnectionFactory.CreateConnection();
        _fileMetasChannel = _fileMetasConnection.CreateModel();
    }

    public void ConsumeFileMeta()
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

                if (!data.HasValue) throw new Exception("Data is invalid");
                _logger.LogDebug("data of file meta: {Data}", data);


                using var scope = ServiceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                try
                {
                    var meta = JsonSerializer.Deserialize<SoundFileMeta>(data!) ?? throw new Exception();
                    if (meta is not { Album: { }, Author: { } })
                        throw new Exception();

                    await unitOfWork.SoundFileMetas.CreateAsync(meta);
                    _logger.LogInformation("Added Sound");
                }
                catch
                {
                    try
                    {
                        var meta = JsonSerializer.Deserialize<VideoFileMeta>(data!) ?? throw new Exception();
                        if (meta is not { Producer: { }, Studio: { } })
                            throw new Exception();

                        await unitOfWork.VideoFileMetas.CreateAsync(meta);
                        _logger.LogInformation("Added Video");
                    }
                    catch
                    {
                        var meta = JsonSerializer.Deserialize<TextFileMeta>(data!) ?? throw new Exception();

                        await unitOfWork.TextFileMetas.CreateAsync(meta);
                        _logger.LogInformation("Added Text");
                    }
                }

                _fileMetasChannel.BasicAck(ea.DeliveryTag, false);
                _logger.LogInformation("Ack");
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