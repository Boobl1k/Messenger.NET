using Back.Services;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly MessagesService _messagesService;

    public MessagesController(MessagesService messagesService) => 
        _messagesService = messagesService;

    [HttpGet]
    public async Task<IActionResult> GetLastHundred() => new JsonResult(await _messagesService.GetLast(100));

    public record MessageInput(string Text, string UserName);

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MessageInput messageInput)
    {
        await _messagesService.Publish(messageInput.UserName, messageInput.Text);
        return Ok();
    }
}