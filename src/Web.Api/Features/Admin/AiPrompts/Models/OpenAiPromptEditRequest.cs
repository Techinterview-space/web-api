using System.ComponentModel.DataAnnotations;
using Domain.Entities.Prompts;

namespace Web.Api.Features.Admin.AiPrompts.Models;

public record OpenAiPromptEditRequest
{
    public OpenAiPromptType Type { get; init; }

    [Required]
    public string Prompt { get; init; }

    [Required]
    public string Model { get; init; }

    [Required]
    public AiEngine Engine { get; init; }
}