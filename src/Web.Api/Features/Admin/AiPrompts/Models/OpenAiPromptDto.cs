using System;
using Domain.Entities.Prompts;

namespace Web.Api.Features.Admin.AiPrompts.Models;

public record OpenAiPromptDto
{
    public Guid Id { get; init; }

    public OpenAiPromptType Type { get; init; }

    public string Prompt { get; init; }

    public string Model { get; init; }

    public AiEngine Engine { get; init; }

    public bool IsActive { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}