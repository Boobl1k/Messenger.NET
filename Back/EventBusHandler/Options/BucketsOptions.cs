namespace EventBusHandler.Options;

public class BucketsOptions
{
    public const string Buckets = "BucketsConfiguration";

    public string TempBucketName { get; set; } = null!;
    public string PermBucketName { get; set; } = null!;
}