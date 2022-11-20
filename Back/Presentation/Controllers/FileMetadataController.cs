using Domain.File;
using Microsoft.AspNetCore.Mvc;
using Presentation.Dto;
using Presentation.Services;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class FileMetadataController : ControllerBase
{
    private readonly CacheService _cacheService;
    private readonly FilesService _filesService;

    public FileMetadataController(CacheService cacheService, FilesService filesService)
    {
        _cacheService = cacheService;
        _filesService = filesService;
    }

    private const string SoundEndpoint = "Sound";

    [HttpPost(SoundEndpoint)]
    public async Task<IActionResult> PostAudioFileMetadata([FromForm] SoundFileMetaDto meta)
    {
        try
        {
            await _filesService.SaveFileMetaAsync(new SoundFileMeta(meta.Id, meta.Name, meta.Album, meta.Author));
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }

    [HttpGet(SoundEndpoint)]
    public async Task<IActionResult> GetAudioFileMetadata([FromQuery] Guid id)
    {
        try
        {
            return Ok(await _cacheService.GetValueAsync<SoundFileMeta>(id));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }
}