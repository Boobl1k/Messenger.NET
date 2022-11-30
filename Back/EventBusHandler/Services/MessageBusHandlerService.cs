using EventBusHandler.Services.Consumer;

namespace EventBusHandler.Services;

internal class MessageBusHandlerService : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(1));
    private readonly ILogger<MessageBusHandlerService> _logger;

    private readonly MessageConsumer _messageConsumer;

    public MessageBusHandlerService(ILogger<MessageBusHandlerService> logger, MessageConsumer messageConsumer)
    {
        _logger = logger;
        _messageConsumer = messageConsumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            try
            {
                _messageConsumer.ConsumeMessage();
            }
            catch (Exception e)
            {
                _logger.LogError("😭 ERROR: \'{ErrorMessage}\'", e.Message);
                _messageConsumer.Dispose();
            }
        } while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }

    public override void Dispose()
    {
        _messageConsumer.Dispose();
        base.Dispose();
    }
}