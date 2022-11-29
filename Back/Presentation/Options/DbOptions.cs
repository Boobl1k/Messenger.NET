namespace Presentation.Options;

public class DbOptions
{
    public const string OptionsPath = "DbConfiguration";
    public string ConnectionString { get; set; } = null!;
}