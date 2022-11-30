using System.Text;
using System.Text.Json;
using Amazon.S3;
using Domain;
using EventBusHandler.Data.Context;
using EventBusHandler.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBusHandler.Services;

public class MessageBusHandlerService : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));
    private readonly ILogger<MessageBusHandlerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly RabbitOptions _rabbitOptions;

    private IConnection? _messagesConnection;
    private readonly AmazonS3Client _s3Client;

    public MessageBusHandlerService(ILogger<MessageBusHandlerService> logger, IServiceProvider serviceProvider,
        IOptions<S3Options> s3Options, IOptions<RabbitOptions> rabbitOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _rabbitOptions = rabbitOptions.Value;

        _s3Client = new AmazonS3Client(s3Options.Value.KeyId, s3Options.Value.SecretKey,
            new AmazonS3Config { ServiceURL = s3Options.Value.ConnectionString, ForcePathStyle = true });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            try
            {
                KillConnections();
                DoWork();
            }
            catch (Exception e)
            {
                _logger.LogError("😭 ERROR: \'{ErrorMessage}\'", e.Message);
                KillConnections();
            }
        } while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }

    private void DoWork()
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitOptions.HostName,
            DispatchConsumersAsync = true
        };

        _messagesConnection = factory.CreateConnection();
        var channel = _messagesConnection.CreateModel();
        channel.QueueDeclare(queue: _rabbitOptions.MessagesQueue, durable: true, exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("{Now}: Messages connection opened", DateTime.Now);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<Message>(content);
            if (message is null)
            {
                _logger.LogInformation(@"Broken message: '{Content}'", content);
                return;
            }

            _logger.LogInformation(@"New message: {MessageUserName} says '{MessageText}'", message.UserName,
                message.Text);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Messages.Add(message);
            await context.SaveChangesAsync();

            channel.BasicAck(ea.DeliveryTag, false);
            await Task.Yield();
        };
        channel.BasicConsume(_rabbitOptions.MessagesQueue, false, consumer);
    }

    public override void Dispose()
    {
        KillConnections();
        _s3Client.Dispose();
        base.Dispose();
    }

    private void KillConnections()
    {
        _logger.LogWarning("{Now}: Closing connections", DateTime.Now);

        if (_messagesConnection?.IsOpen ?? false)
            _messagesConnection?.Close();
    }
}