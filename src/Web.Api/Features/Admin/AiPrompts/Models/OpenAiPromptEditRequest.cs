using System.ComponentModel.DataAnnotations;
using Domain.Entities.OpenAI;

namespace Web.Api.Features.Admin.AiPrompts.Models;

public record OpenAiPromptEditRequest
{
    public OpenAiPromptType? Id { get; init; }

    [Required]
    public string Prompt { get; init; }

    [Required]
    public string Model { get; init; }

    [Required]
    public AiEngine Engine { get; init; }
}