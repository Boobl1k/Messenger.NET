using System.Globalization;
using System.Text;
using RabbitMQ.Client;

namespace Presentation.RabbitMQ.Consumers;

public class AdminConsumer : IDisposable
{
    private readonly IModel _channel;
    private readonly ILogger<AdminConsumer> _logger;

    public AdminConsumer(ILogger<AdminConsumer> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory { HostName = "rabbit" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
    }

    public string ConsumeAdmin()
    {
        _channel.QueueDeclare(queue: "admin", durable: true, exclusive: false,
            autoDelete: false,
            arguments: null);

        var res = _channel.BasicGet("admin", true);

        if (res is null)
        {
            _logger.LogError("cannot consume admin");
            throw new Exception("cannot consume admin");
        }

        var adminName = Encoding.UTF8.GetString(res.Body.ToArray());
        _logger.LogInformation("got admin name \"{admin name}\"", adminName);

        return adminName;
    }

    public void Dispose() => _channel.Dispose();
}