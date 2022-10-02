using Back.Contracts;
using Back.Services;
using MassTransit;

namespace Back.Cucumbers;

public class DefaultCucumber : ICucumber<MessageContract>
{
    private readonly ILogger<DefaultCucumber> _logger;
    private readonly MessagesService _messagesService;

    public DefaultCucumber(ILogger<DefaultCucumber> logger, MessagesService messagesService)
    {
        _logger = logger;
        _messagesService = messagesService;
    }

    public async Task Consume(ConsumeContext<MessageContract> context)
    {
        _logger.LogInformation($"{context.Message.UserName} sends '{context.Message.Text}'");
        await _messagesService.AddMessage(context.Message.Text, context.Message.UserName);
    }
}