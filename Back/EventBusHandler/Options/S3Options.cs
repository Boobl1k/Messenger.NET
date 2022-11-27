namespace EventBusHandler.Options;

public class S3Options
{
    public const string S3 = "S3Configuration";

    public string KeyId { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string ConnectionString { get; set; }  = null!;
}