using Amazon.S3;
using Amazon.S3.Model;

namespace Presentation.Services;

public record FileData(Stream Stream, string ContentType, string Name);

public class FilesService
{
    private const string BucketName = "defaultbucket";
    private const string OriginalFilenameMetadataField = "original-filename";
    private readonly AmazonS3Client _client;

    public FilesService()
    {
        _client = new AmazonS3Client("qweqweqwe", "qweqweqwe",
            new AmazonS3Config() { ServiceURL = "http://minio:9000", ForcePathStyle = true });
    }

    public async Task<Guid> SaveFileAsync(Stream stream, string filename, string contentType)
    {
        if (!(await _client.ListBucketsAsync()).Buckets.Exists(b => b.BucketName == BucketName))
            await _client.PutBucketAsync(BucketName);
        
        var id = Guid.NewGuid();
        var encodedFilename = Uri.EscapeDataString(filename);
        var request = new PutObjectRequest()
        {
            BucketName = BucketName,
            InputStream = stream,
            AutoCloseStream = true,
            Key = id.ToString(),
            ContentType = contentType,
            Headers =
            {
                ContentDisposition = $"attachment; filename=\"{encodedFilename}\""
            },
        };
        request.Metadata.Add(OriginalFilenameMetadataField, encodedFilename);
        
        await _client.PutObjectAsync(request);

        return id;
    }

    public async Task<FileData> ReadFileAsync(Guid fileId)
    {
        var request = new GetObjectRequest()
        {
            Key = fileId.ToString(),
            BucketName = BucketName
        };
        var response = await _client.GetObjectAsync(request);

        var filename = response.Metadata[OriginalFilenameMetadataField];

        return new FileData(
            response.ResponseStream,
            response.Headers.ContentType,
            filename
        );
    }
}