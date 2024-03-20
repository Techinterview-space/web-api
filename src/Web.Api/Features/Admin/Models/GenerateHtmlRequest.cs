using System.ComponentModel.DataAnnotations;

namespace TechInterviewer.Features.Admin.Models;

public record GenerateHtmlRequest
{
    [Required]
    public string Content { get; init; }
}