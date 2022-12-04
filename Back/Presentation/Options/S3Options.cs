namespace Presentation.Options;

public class S3Options
{
    public const string OptionsPath = "S3Configuration";

    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Url { get; set; } = null!;
}