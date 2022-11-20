using System.Text;
using Domain;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Presentation.RabbitMQ.Producer;

public class MessagesProducer
{
    private readonly ILogger<MessagesProducer> _logger;

    public MessagesProducer(ILogger<MessagesProducer> logger) =>
        _logger = logger;

    public void ProduceMessage(Message message)
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        var connection = factory.CreateConnection();
        _logger.LogInformation("RabbitMQ: Connection established");
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "messages",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(json);
        channel.BasicPublish(exchange: "", routingKey: "messages", body: body);
    }
}