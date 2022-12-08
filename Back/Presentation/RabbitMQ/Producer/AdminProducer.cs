using System.Text;
using RabbitMQ.Client;

namespace Presentation.RabbitMQ.Producer;

public class AdminProducer : IDisposable
{
    private readonly IModel _channel;

    public AdminProducer()
    {
        var factory = new ConnectionFactory { HostName = "rabbit" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
    }

    public void ProduceAdminFreeCommand(string adminName)
    {
        _channel.QueueDeclare(queue: "admin",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(adminName);
        _channel.BasicPublish(exchange: "", routingKey: "admin", body: body);
    }

    public void Dispose() => _channel.Dispose();
}