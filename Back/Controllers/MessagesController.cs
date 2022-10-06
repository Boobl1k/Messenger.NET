using Back.Contracts;
using Back.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly MessagesService _messagesService;
    private readonly IPublishEndpoint _publishEndpoint;

    public MessagesController(MessagesService messagesService, IPublishEndpoint publishEndpoint)
    {
        _messagesService = messagesService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<IActionResult> GetLastHundred() => new JsonResult(await _messagesService.GetLast(100));

    public record MessageInput(string Text, string UserName);

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MessageInput messageInput)
    {
        await _publishEndpoint.Publish(new MessageContract(messageInput.UserName, messageInput.Text));
        return Ok();
    }
}