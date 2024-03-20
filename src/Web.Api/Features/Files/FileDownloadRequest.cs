using System.ComponentModel.DataAnnotations;

namespace TechInterviewer.Features.Files;

public record FileDownloadRequest
{
    [Required]
    public string Filename { get; init; }
}