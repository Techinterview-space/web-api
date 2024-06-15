using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.Files;

public record FileDownloadRequest
{
    [Required]
    public string Filename { get; init; }
}