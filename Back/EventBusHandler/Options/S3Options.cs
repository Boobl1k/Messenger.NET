namespace EventBusHandler.Options;

public class S3Options
{
    public const string S3Configuration = "S3Configuration";

    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string ConnectionString { get; set; }  = null!;
}