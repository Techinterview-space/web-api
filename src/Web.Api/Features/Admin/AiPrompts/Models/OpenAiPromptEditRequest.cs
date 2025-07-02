using System.ComponentModel.DataAnnotations;
using Domain.Entities.OpenAI;

namespace Web.Api.Features.Admin.AiPrompts.Models;

public record OpenAiPromptEditRequest
{
    public OpenAiPromptType? Id { get; init; }

    [Required]
    [StringLength(2000)]
    public string Prompt { get; init; }

    [Required]
    [StringLength(100)]
    public string Model { get; init; }

    [Required]
    public AiEngine Engine { get; init; }
}