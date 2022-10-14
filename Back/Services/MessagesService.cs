using Back.Entities;
using Back.RabbitMQ.Producer;
using Back.Repositories;
using MassTransit;

namespace Back.Services;

public class MessagesService
{
    private readonly MessagesRepository _messagesRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMessageProducer _messagePublisher;

    public MessagesService(MessagesRepository messagesRepository, IPublishEndpoint publishEndpoint,
        IMessageProducer messagePublisher)
    {
        _messagesRepository = messagesRepository;
        _publishEndpoint = publishEndpoint;
        _messagePublisher = messagePublisher;
    }

    public async Task<IEnumerable<Message>> GetLast(int count = 20) => await _messagesRepository.GetLast(count);

    public async Task<Message> AddMessage(Message message)
    {
        _messagePublisher.SendMessage(message);

        await _publishEndpoint.Publish(message);
        return message;
    }

    public async Task<bool> RemoveAll() =>
        await _messagesRepository.RemoveAll();
}