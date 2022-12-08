using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MyBook.Entity;

namespace MyBook.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly UserManager<User> _userManager;

    public ChatHub(UserManager<User> userManager) => 
        _userManager = userManager;

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.Name);
        
        if (Context.User.IsInRole("Admin"))
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
        
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string message)
    {
        var sender = await _userManager.FindByEmailAsync(Context.User.Identity.Name);
        await Clients.Group("Admin").SendAsync("ReceiveMessage", sender.Email, sender.Image, null, message);

        if (!Context.User.IsInRole("Admin"))
            await Clients.Caller.SendAsync("ReceiveMessage",sender.Email, sender.Image, null, message);
    }

    [Authorize(Roles = "Admin")]
    public async Task SendMessageToGroup(string receiver, string message)
    {
        var user = await _userManager.FindByEmailAsync(receiver);
        var sender = await _userManager.FindByEmailAsync(Context.User.Identity.Name);
        
        if (user is not null)
            await Clients.Group(receiver).SendAsync("ReceiveMessage",sender.Email, sender.Image, receiver, message);
        else
            await Clients.Caller.SendAsync("Notify", "Пользователь не существует. Сообщение не доставлено.");

        await Clients.Caller.SendAsync("ReceiveMessage",sender.Email, sender.Image, receiver, message);
    }
}