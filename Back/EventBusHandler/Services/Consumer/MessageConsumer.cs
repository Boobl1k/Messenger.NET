using System.Text;
using System.Text.Json;
using Domain;
using EventBusHandler.Data.Context;
using EventBusHandler.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBusHandler.Services.Consumer;

internal class MessageConsumer : ConsumerBase, IDisposable
{
    private readonly ILogger<MessageConsumer> _logger;

    private readonly IConnection? _messagesConnection;
    private readonly IModel _messagesChannel;

    public MessageConsumer(IOptions<RabbitOptions> rabbitOptions, ILogger<MessageConsumer> logger,
        IServiceProvider serviceProvider, ConnectionFactory connectionFactory) : base(rabbitOptions, serviceProvider, connectionFactory)
    {
        _logger = logger;

        _messagesConnection = ConnectionFactory.CreateConnection();
        _messagesChannel = _messagesConnection.CreateModel();
    }

    public void StartConsumingMessages()
    {
        _messagesChannel.QueueDeclare(queue: RabbitOptions.MessagesQueue, durable: true, exclusive: false,
            autoDelete: false,
            arguments: null);
        
        _logger.LogInformation("{Now}: Messages connection opened", DateTime.Now);

        var consumer = new AsyncEventingBasicConsumer(_messagesChannel);
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

            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Messages.Add(message);
            await context.SaveChangesAsync();

            _messagesChannel.BasicAck(ea.DeliveryTag, false);
            await Task.Yield();
        };
        _messagesChannel.BasicConsume(RabbitOptions.MessagesQueue, false, consumer);
    }

    public void Dispose()
    {
        _messagesChannel.Dispose();
        
        if (_messagesConnection?.IsOpen ?? false)
            _messagesConnection.Dispose();
    }
}