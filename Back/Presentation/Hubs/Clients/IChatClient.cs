using Domain;

namespace Presentation.Hubs.Clients;

public interface IChatClient
{
    Task ReceiveMessage(Message message);
}