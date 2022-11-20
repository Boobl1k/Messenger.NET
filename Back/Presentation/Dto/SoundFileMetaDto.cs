using System.ComponentModel.DataAnnotations;

namespace Presentation.Dto;

public record SoundFileMetaDto([Required] Guid Id, [Required] string Name, [Required] string Author,
    [Required] string Album);