using System.ComponentModel.DataAnnotations;

namespace TechInterviewer.Controllers.Files;

public record FileDownloadRequest
{
    [Required]
    public string Filename { get; init; }
}