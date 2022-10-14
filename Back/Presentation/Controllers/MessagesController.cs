using Microsoft.AspNetCore.Mvc;
using Presentation.RabbitMQ.Producer;
using Presentation.Services;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly MessagesService _messagesService;

    public MessagesController(MessagesService messagesService, IMessageProducer messagePublisher) =>
        _messagesService = messagesService;

    [HttpGet]
    public async Task<IActionResult> GetLastHundred() => new JsonResult(await _messagesService.GetLast(100));

    [HttpDelete]
    public async Task<IActionResult> ResetChat() =>
        await _messagesService.RemoveAll() ? Ok() : new StatusCodeResult(500);
}