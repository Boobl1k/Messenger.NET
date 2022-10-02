using Back.Entities;
using Back.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Back.Hubs;

public class ChatHub : Hub<IChatClient>
{
    public async Task SendMessage(Message message) => 
        await Clients.All.ReceiveMessage(message);
}