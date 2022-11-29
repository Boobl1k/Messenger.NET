using System.Text;
using RabbitMQ.Client;

namespace Presentation.RabbitMQ.Producer;

public class FileSaveCommandsProducer : IDisposable
{
    private readonly IModel _channel;

    public FileSaveCommandsProducer()
    {
        var factory = new ConnectionFactory { HostName = "rabbit" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
    }

    public void ProduceFileSaveCommand(Guid fileId)
    {
        _channel.QueueDeclare(queue: "fileMetas",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(fileId.ToString());
        _channel.BasicPublish(exchange: "", routingKey: "fileMetas", body: body);
    }

    public void Dispose() => _channel.Dispose();
}