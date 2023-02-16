using System.ComponentModel.DataAnnotations;

namespace TechInterviewer.Controllers.Admin.Dtos;

public record GenerateHtmlRequest
{
    [Required]
    public string Content { get; init; }
}