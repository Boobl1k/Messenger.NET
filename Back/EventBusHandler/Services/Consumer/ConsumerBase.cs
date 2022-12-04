using EventBusHandler.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EventBusHandler.Services.Consumer;

internal class ConsumerBase
{
    protected readonly ConnectionFactory ConnectionFactory;
    protected readonly RabbitOptions RabbitOptions;
    protected readonly IServiceProvider ServiceProvider;

    protected ConsumerBase(IOptions<RabbitOptions> rabbitOptions, IServiceProvider serviceProvider,
        ConnectionFactory connectionFactory)
    {
        ServiceProvider = serviceProvider;
        ConnectionFactory = connectionFactory;
        RabbitOptions = rabbitOptions.Value;
    }
}