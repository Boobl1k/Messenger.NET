using Domain;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs.Clients;
using Presentation.Services;

namespace Presentation.Hubs;

public class ChatHub : Hub<IChatClient>
{
    private readonly MessagesService _messagesService;
    private readonly AdminService _adminService;

    public ChatHub(MessagesService messagesService, AdminService adminService)
    {
        _messagesService = messagesService;
        _adminService = adminService;
    }

    public async Task SendMessage(string userName, Message message)
    {
        await Task.WhenAll(_messagesService.AddMessage(message), Clients.All.ReceiveMessage(userName, message));
    }
    
    public async Task FreeAdmin(string adminName)
    {
        await Clients.All.GoWait(adminName);
    }
}