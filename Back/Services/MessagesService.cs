using Back.Contracts;
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

    public async Task<Message?> AddMessage(string messageText, string userName)
    {
        var message = new Message
        {
            Text = messageText,
            UserName = userName,
            DateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
        };

        return await _messagesRepository.AddMessage(message) ? message : null;
    }
}