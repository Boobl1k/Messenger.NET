using Microsoft.AspNetCore.Mvc;
using Presentation.Services;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]/{id:guid?}")]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;
    private readonly FilesService _filesService;

    public FilesController(FilesService filesService, ILogger<FilesController> logger)
    {
        _filesService = filesService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostFile([FromForm] IFormFile file, [FromRoute] Guid id)
    {
        try
        {
            return Ok(await _filesService.SaveFileAsync(file.OpenReadStream(), file.ContentType, id));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "cannot save file");
            return BadRequest();
        }
    }
}