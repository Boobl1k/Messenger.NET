using Domain.File;
using Microsoft.AspNetCore.Mvc;
using Presentation.Dto;
using Presentation.Services;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    private const string TextEndpoint = "Text";
    private const string VideoEndpoint = "Video";

    #region sound

    [HttpPost(SoundEndpoint)]
    public async Task<IActionResult> PostAudioFileMetadata([FromBody] SoundFileMetaDto meta)
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

    #endregion

    #region text

    [HttpPost(TextEndpoint)]
    public async Task<IActionResult> PostTextFileMetadata([FromBody] TextFileMetaDto meta)
    {
        try
        {
            await _filesService.SaveFileMetaAsync(new TextFileMeta(meta.Id, meta.Name));
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }

    [HttpGet(TextEndpoint)]
    public async Task<IActionResult> GetTextFileMetadata([FromQuery] Guid id)
    {
        try
        {
            return Ok(await _cacheService.GetValueAsync<TextFileMeta>(id));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }

    #endregion

    #region video

    [HttpPost(VideoEndpoint)]
    public async Task<IActionResult> PostVideoFileMetadata([FromBody] VideoFileMetaDto meta)
    {
        try
        {
            await _filesService.SaveFileMetaAsync(new VideoFileMeta(meta.Id, meta.Name, meta.Extension, meta.Studio,
                meta.Producer));
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }

    [HttpGet(VideoEndpoint)]
    public async Task<IActionResult> GetVideoFileMetadata([FromQuery] Guid id)
    {
        try
        {
            return Ok(await _cacheService.GetValueAsync<VideoFileMeta>(id));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }

    #endregion
}