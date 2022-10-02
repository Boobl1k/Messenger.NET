using Back.Contracts;
using Back.Entities;
using Back.Hubs;
using Back.Hubs.Clients;
using Back.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly MessagesService _messagesService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHubContext<ChatHub, IChatClient> _chatHub;

    public MessagesController(MessagesService messagesService, IPublishEndpoint publishEndpoint,
        IHubContext<ChatHub, IChatClient> chatHub)
    {
        _messagesService = messagesService;
        _publishEndpoint = publishEndpoint;
        _chatHub = chatHub;
    }

    [HttpGet]
    public async Task<IActionResult> GetLastHundred() => new JsonResult(await _messagesService.GetLast(100));

    public record MessageInput(string Text, string UserName);

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MessageInput messageInput)
    {
        await _chatHub.Clients.All.ReceiveMessage(new Message
            { Text = messageInput.Text, UserName = messageInput.UserName });

        await _publishEndpoint.Publish(new MessageContract(messageInput.UserName, messageInput.Text));
        return Ok();
    }
}