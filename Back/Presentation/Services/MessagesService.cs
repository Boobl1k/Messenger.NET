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

    public async Task<IEnumerable<Message>> GetLastForUser(string username, int count = 20) =>
        await _messagesRepository.GetLastForUser(username, count);

    public Task<Message> AddMessage(Message message)
    {
        _messagePublisher.ProduceMessage(message);
        return Task.FromResult(message);
    }

    public async Task<bool> RemoveAll() =>
        await _messagesRepository.RemoveAll();
}