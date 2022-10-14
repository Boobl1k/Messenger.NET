using Domain;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs.Clients;
using Presentation.Services;

namespace Presentation.Hubs;

public class ChatHub : Hub<IChatClient>
{
    private readonly MessagesService _messagesService;

    public ChatHub(MessagesService messagesService) => 
        _messagesService = messagesService;

    public async Task SendMessage(Message message) => 
        await Clients.All.ReceiveMessage(await _messagesService.AddMessage(message));
}