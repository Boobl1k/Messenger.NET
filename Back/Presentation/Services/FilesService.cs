using Amazon.S3;
using Amazon.S3.Model;

namespace Presentation.Services;

public record FileData(Stream Stream, string ContentType, Guid Id);

public class FilesService
{
    private const string BucketName = "tempbucket";
    private readonly AmazonS3Client _client;

    public FilesService()
    {
        _client = new AmazonS3Client("qweqweqwe", "qweqweqwe",
            new AmazonS3Config { ServiceURL = "http://minio:9000", ForcePathStyle = true });
    }

    public async Task<Guid> SaveFileAsync(Stream stream, string contentType, Guid id)
    {
        if (!(await _client.ListBucketsAsync()).Buckets.Exists(b => b.BucketName == BucketName))
            await _client.PutBucketAsync(BucketName);

        var request = new PutObjectRequest
        {
            BucketName = BucketName,
            InputStream = stream,
            AutoCloseStream = true,
            Key = id.ToString(),
            ContentType = contentType
        };

        await _client.PutObjectAsync(request);

        return id;
    }

    public async Task<FileData> ReadFileAsync(Guid fileId)
    {
        var request = new GetObjectRequest
        {
            Key = fileId.ToString(),
            BucketName = BucketName
        };
        var response = await _client.GetObjectAsync(request);

        return new FileData(
            response.ResponseStream,
            response.Headers.ContentType,
            fileId
        );
    }
}