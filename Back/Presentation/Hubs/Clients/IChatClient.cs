using Domain;

namespace Presentation.Hubs.Clients;

public interface IChatClient
{
    Task ReceiveMessage(string userName, Message message);
    Task FreeAdmin(string adminName);
    Task GoChat(string adminName, string userName);
    Task GoWait(string adminName);
}