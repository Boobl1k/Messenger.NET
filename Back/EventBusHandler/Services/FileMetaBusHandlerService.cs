using System.Text;
using System.Text.Json;
using Amazon.S3;
using Domain.File;
using EventBusHandler.Data;
using EventBusHandler.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBusHandler.Services;

public class FileMetaBusHandlerService : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));
    private readonly ILogger<FileMetaBusHandlerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly RedisCacheService _redisCacheService;

    private readonly BucketsOptions _bucketsOptions;
    private readonly RabbitOptions _rabbitOptions;

    private IConnection? _fileMetasConnection;
    private readonly AmazonS3Client _s3Client;

    public FileMetaBusHandlerService(ILogger<FileMetaBusHandlerService> logger, IServiceProvider serviceProvider,
        IOptions<BucketsOptions> bucketOptions, IOptions<S3Options> s3Options, IOptions<RabbitOptions> rabbitOptions,
        RedisCacheService redisCacheService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _redisCacheService = redisCacheService;

        _bucketsOptions = bucketOptions.Value;
        _rabbitOptions = rabbitOptions.Value;

        _s3Client = new AmazonS3Client(s3Options.Value.KeyId, s3Options.Value.SecretKey,
            new AmazonS3Config { ServiceURL = s3Options.Value.ConnectionString, ForcePathStyle = true });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            try
            {
                KillConnections();
                await DoWorkAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("😭 ERROR: \'{ErrorMessage}\'", e.Message);
                KillConnections();
            }
        } while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }

    private async Task DoWorkAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitOptions.HostName,
            DispatchConsumersAsync = true
        };

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

                _logger.LogDebug("got new file id={FileId} from bus", fileId);

                if (!(await _s3Client.ListBucketsAsync()).Buckets.Exists(b =>
                        b.BucketName == _bucketsOptions.PermBucketName))
                    await _s3Client.PutBucketAsync(_bucketsOptions.PermBucketName);

                await _s3Client.CopyObjectAsync(_bucketsOptions.TempBucketName, fileId,
                    _bucketsOptions.PermBucketName,
                    fileId);

                var data = await _redisCacheService.GetAsync(fileId);

                if (!data.HasValue) throw new Exception("Data is invalid");
                _logger.LogDebug("data of file meta: {Data}", data);


                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                try
                {
                    var meta = JsonSerializer.Deserialize<SoundFileMeta>(data!) ?? throw new Exception();
                    if (meta is not { Album: { }, Author: { } })
                        throw new Exception();

                    await unitOfWork.SoundFileMetas.CreateAsync(meta);
                }
                catch
                {
                    try
                    {
                        var meta = JsonSerializer.Deserialize<VideoFileMeta>(data!) ?? throw new Exception();
                        if (meta is not { Producer: { }, Studio: { } })
                            throw new Exception();

                        await unitOfWork.VideoFileMetas.CreateAsync(meta);
                    }
                    catch
                    {
                        var meta = JsonSerializer.Deserialize<TextFileMeta>(data!) ?? throw new Exception();

                        await unitOfWork.TextFileMetas.CreateAsync(meta);
                    }
                }

                channel.BasicAck(ea.DeliveryTag, false);
                await Task.Yield();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "😭 error in FileMetaBusHandlerService");
            }
        };
        channel.BasicConsume(_rabbitOptions.FileMetasQueue, false, consumer);
    }

    public override void Dispose()
    {
        KillConnections();
        _s3Client.Dispose();
        base.Dispose();
    }

    private void KillConnections()
    {
        _logger.LogWarning("{Now}: Closing connections", DateTime.Now);

        if (_fileMetasConnection?.IsOpen ?? false)
            _fileMetasConnection?.Close();
    }
}