using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.Admin.Models;

public record GenerateHtmlRequest
{
    [Required]
    public string Content { get; init; }
}