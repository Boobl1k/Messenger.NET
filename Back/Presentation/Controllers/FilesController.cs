using Microsoft.AspNetCore.Mvc;
using Presentation.Services;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly FilesService _filesService;

    public FilesController(FilesService filesService) =>
        _filesService = filesService;

    [HttpGet]
    public async Task<IActionResult> GetFile(Guid id)
    {
        Console.WriteLine(id);
        try
        {
            var file = await _filesService.ReadFileAsync(id);
            return File(file.Stream, file.ContentType, file.Id.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostFile([FromForm] IFormFile file, Guid id)
    {
        try
        {
            return Ok(await _filesService.SaveFileAsync(file.OpenReadStream(), file.ContentType, id));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }
}