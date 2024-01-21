using Domain.ValueObjects;

namespace TechInterviewer.Controllers.Skills.Dtos;

public record SkillDto
{
    public long Id { get; init; }

    public string Title { get; init; }

    public HexColor HexColor { get; init; }
}