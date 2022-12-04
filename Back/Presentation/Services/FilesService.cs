using Amazon.S3;
using Amazon.S3.Model;
using Domain.File;
using Presentation.RabbitMQ.Producer;
using StackExchange.Redis;

namespace Presentation.Services;

public record FileData(Stream Stream, string ContentType, Guid Id);

public class FilesService
{
    private const string BucketName = "tempbucket";

    private readonly AmazonS3Client _s3Client;
    private readonly CacheService _cacheService;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly FileSaveCommandsProducer _fileSaveCommandsProducer;

    public FilesService(AmazonS3Client s3Client, CacheService cacheService, IConnectionMultiplexer redisConnection,
        FileSaveCommandsProducer fileSaveCommandsProducer)
    {
        _s3Client = s3Client;
        _cacheService = cacheService;
        _redisConnection = redisConnection;
        _fileSaveCommandsProducer = fileSaveCommandsProducer;
    }

    public async Task<Guid> SaveFileAsync(Stream stream, string contentType, Guid id)
    {
        if (!(await _s3Client.ListBucketsAsync()).Buckets.Exists(b => b.BucketName == BucketName))
            await _s3Client.PutBucketAsync(BucketName);

        var request = new PutObjectRequest
        {
            BucketName = BucketName,
            InputStream = stream,
            AutoCloseStream = true,
            Key = id.ToString(),
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request);
        await HandleFileMoving(id);

        return id;
    }

    public async Task<FileData> ReadFileAsync(Guid fileId)
    {
        var request = new GetObjectRequest
        {
            Key = fileId.ToString(),
            BucketName = BucketName
        };
        var response = await _s3Client.GetObjectAsync(request);

        return new FileData(
            response.ResponseStream,
            response.Headers.ContentType,
            fileId
        );
    }

    public async Task SaveFileMetaAsync(IFileMeta meta)
    {
        await _cacheService.SetValueAsync(meta.Id, meta);
        await HandleFileMoving(meta.Id);
    }

    public async Task<T> GetFileMetaAsync<T>(Guid id) where T : class, IFileMeta =>
        await _cacheService.GetValueAsync<T>(id);

    private async Task HandleFileMoving(Guid id)
    {
        if (await _redisConnection.GetDatabase().StringIncrementAsync(
                BitConverter.ToInt32(id.ToByteArray(), 0).ToString()) > 1)
            _fileSaveCommandsProducer.ProduceFileSaveCommand(id);
    }
}