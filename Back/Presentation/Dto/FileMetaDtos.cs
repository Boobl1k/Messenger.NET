using System.ComponentModel.DataAnnotations;
using Domain.File;

namespace Presentation.Dto;

public record SoundFileMetaDto([Required] Guid Id, [Required] string Name, [Required] string Author,
    [Required] string Album);

public record TextFileMetaDto([Required] Guid Id, [Required] string Name);

public record VideoFileMetaDto([Required] Guid Id, [Required] string Name,
    [Required] VideoFileMeta.VideoFileExtension Extension,
    [Required] string Studio, [Required] string Producer);