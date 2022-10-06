using Back.Entities;
using Back.Repositories;
using MassTransit;

namespace Back.Cucumbers;

// ReSharper disable once ClassNeverInstantiated.Global
public class DefaultCucumber : ICucumber<Message>
{
    private readonly ILogger<DefaultCucumber> _logger;
    private readonly MessagesRepository _messagesRepository;

    public DefaultCucumber(ILogger<DefaultCucumber> logger, MessagesRepository messagesRepository)
    {
        _logger = logger;
        _messagesRepository = messagesRepository;
    }

    public async Task Consume(ConsumeContext<Message> context)
    {
        _logger.LogInformation($"{context.Message.UserName} sends '{context.Message.Text}'");
        await _messagesRepository.AddMessage(context.Message);
    }
}