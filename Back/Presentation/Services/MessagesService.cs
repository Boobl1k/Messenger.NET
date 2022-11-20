using Domain;
using Presentation.RabbitMQ.Producer;
using Presentation.Repositories;

namespace Presentation.Services;

public class MessagesService
{
    private readonly MessagesRepository _messagesRepository;
    private readonly MessagesProducer _messagePublisher;

    public MessagesService(MessagesRepository messagesRepository,
        MessagesProducer messagePublisher)
    {
        _messagesRepository = messagesRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task<IEnumerable<Message>> GetLast(int count = 20) => await _messagesRepository.GetLast(count);

    public async Task<Message> AddMessage(Message message)
    {
        _messagePublisher.ProduceMessage(message);
        return message;
    }

    public async Task<bool> RemoveAll() =>
        await _messagesRepository.RemoveAll();
}