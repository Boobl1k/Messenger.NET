using Domain;

namespace Presentation.Hubs.Clients;

public interface IChatClient
{
    Task ReceiveMessage(string userName, Message message);
}