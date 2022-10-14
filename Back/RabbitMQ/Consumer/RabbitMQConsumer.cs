using System.Text;
using Back.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Back.RabbitMQ.Consumer;

public class RabbitMqConsumer : IMessageConsumer
{
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(ILogger<RabbitMqConsumer> logger) => 
        _logger = logger;

    public void ReceiveMessage(Message message)
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

        channel.BasicQos(prefetchSize: 0, prefetchCount: 3, global: false);
        
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var msg = Encoding.UTF8.GetString(body);
            Console.WriteLine("From rabbit:");
            Console.WriteLine(msg);
        };
        channel.BasicConsume(queue: "messages", autoAck: true, consumer: consumer);
    }
}