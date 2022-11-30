namespace EventBusHandler.Options;

public class RedisOptions
{
    public const string RedisConfiguration = "RedisConfiguration";

    public string ConnectionString { get; set; } = null!;
}