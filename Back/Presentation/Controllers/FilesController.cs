using Microsoft.AspNetCore.Mvc;
using Presentation.Services;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]/{id:guid?}")]
public class FilesController : ControllerBase
{
    private readonly FilesService _filesService;

    public FilesController(FilesService filesService) =>
        _filesService = filesService;

    [HttpPost]
    public async Task<IActionResult> PostFile([FromForm] IFormFile file, [FromRoute] Guid id)
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