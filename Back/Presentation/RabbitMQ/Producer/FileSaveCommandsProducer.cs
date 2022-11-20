using System.Text;
using RabbitMQ.Client;

namespace Presentation.RabbitMQ.Producer;

public class FileSaveCommandsProducer
{
    private readonly ILogger<FileSaveCommandsProducer> _logger;

    public FileSaveCommandsProducer(ILogger<FileSaveCommandsProducer> logger) =>
        _logger = logger;
    
    public void ProduceFileSaveCommand(Guid fileId)
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        var connection = factory.CreateConnection();
        _logger.LogInformation("RabbitMQ: Connection established");
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "fileMetas",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(fileId.ToString());
        channel.BasicPublish(exchange: "", routingKey: "fileMetas", body: body);
    }
}