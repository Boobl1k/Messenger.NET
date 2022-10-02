using Back.Contracts;
using Back.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

public class MessagesController : ControllerBase
{
    private readonly MessagesService _messagesService;
    private readonly IPublishEndpoint _publishEndpoint;

    public MessagesController(MessagesService messagesService, IPublishEndpoint publishEndpoint)
    {
        _messagesService = messagesService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet("Messages")]
    public async Task<IActionResult> GetLastHundred() => new JsonResult(await _messagesService.GetLast(100));

    public record MessageInput(string Message, string UserName);

    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] MessageInput message)
    {
        return await _messagesService.AddMessage(message.Message, message.UserName) is { } ? Ok() : StatusCode(500);
    }

    [HttpPost("SendMessageMt")]
    public async Task<IActionResult> SendMessageMt([FromBody] MessageInput messageInput)
    {
        await _publishEndpoint.Publish(new MessageContract(messageInput.UserName, messageInput.Message));
        return Ok();
    }
}