namespace EventBusHandler.Options;

public class MongoOptions
{
    public const string MongoConfiguration = "MongoConfiguration";

    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;

    public string TextCollectionName { get; set; } = null!;
    public string SoundCollectionName { get; set; } = null!;
    public string VideoCollectionName { get; set; } = null!;
}