using System;
using Domain.Entities.OpenAI;

namespace Web.Api.Features.Admin.AiPrompts.Models;

public record OpenAiPromptDto
{
    public OpenAiPromptType Id { get; init; }

    public string Prompt { get; init; }

    public string Model { get; init; }

    public AiEngine Engine { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}