using Back.Entities;
using Back.Repositories;
using MassTransit;

namespace Back.Services;

public class MessagesService
{
    private readonly MessagesRepository _messagesRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public MessagesService(MessagesRepository messagesRepository, IPublishEndpoint publishEndpoint)
    {
        _messagesRepository = messagesRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IEnumerable<Message>> GetLast(int count = 20) => await _messagesRepository.GetLast(count);

    public async Task<Message> AddMessage(Message message)
    {
        await _publishEndpoint.Publish(message);
        return message;
    }

    public async Task<bool> RemoveAll() => 
        await _messagesRepository.RemoveAll();
}