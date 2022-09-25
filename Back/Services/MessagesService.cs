using Back.Entities;
using Back.Repositories;

namespace Back.Services;

public class MessagesService
{
    private readonly MessagesRepository _messagesRepository;

    public MessagesService(MessagesRepository messagesRepository) => _messagesRepository = messagesRepository;

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