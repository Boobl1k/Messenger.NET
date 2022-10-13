using System.Text;
using Back.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Back.RabbitMQ.Producer;

// ReSharper disable once InconsistentNaming
public class RabbitMQProducer : IMessageProducer
{
    private readonly ILogger<RabbitMQProducer> _logger;

    public RabbitMQProducer(ILogger<RabbitMQProducer> logger) =>
        _logger = logger;

    public void SendMessage(Message message)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
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
        channel.BasicPublish(exchange: "messages", routingKey: "messages", body: body);
    }
}