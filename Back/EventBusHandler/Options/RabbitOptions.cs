namespace EventBusHandler.Options;

public class RabbitOptions
{
    public const string RabbitConfiguration = "RabbitConfiguration";

    public string HostName { get; set; } = null!;
    public string MessagesQueue { get; set; } = null!;
    public string FileMetasQueue { get; set; } = null!;
}