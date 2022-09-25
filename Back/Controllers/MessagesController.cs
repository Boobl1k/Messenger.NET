using Back.Services;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

public class MessagesController : ControllerBase
{
    private readonly MessagesService _messagesService;

    public MessagesController(MessagesService messagesService) => _messagesService = messagesService;

    [HttpGet("Messages")]
    public async Task<IActionResult> GetLastHundred() => new JsonResult(await _messagesService.GetLast(100));

    public record MessageInput(string Message, string UserName);
    
    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] MessageInput message)
    {
        return await _messagesService.AddMessage(message.Message, message.UserName) is { } ? Ok() : StatusCode(500);
    }
}