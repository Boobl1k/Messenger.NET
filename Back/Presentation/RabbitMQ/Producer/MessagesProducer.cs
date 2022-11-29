using System.Text;
using Domain;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Presentation.RabbitMQ.Producer;

public class MessagesProducer : IDisposable
{
    private readonly IModel _channel;

    public MessagesProducer()
    {
        var factory = new ConnectionFactory { HostName = "rabbit" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
    }

    public void ProduceMessage(Message message)
    {
        _channel.QueueDeclare(queue: "messages",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(exchange: "", routingKey: "messages", body: body);
    }

    public void Dispose() => _channel.Dispose();
}