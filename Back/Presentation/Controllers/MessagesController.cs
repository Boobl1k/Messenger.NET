using Microsoft.AspNetCore.Mvc;
using Presentation.Services;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly MessagesService _messagesService;

    public MessagesController(MessagesService messagesService) => _messagesService = messagesService;

    [HttpGet]
    public async Task<IActionResult> GetLastHundredForUser([FromQuery] string username) =>
        new JsonResult(await _messagesService.GetLastForUser(username, 100));

    [HttpDelete]
    public async Task<IActionResult> ResetChat() =>
        await _messagesService.RemoveAll() ? Ok() : new StatusCodeResult(500);
}