using Domain.ValueObjects;

namespace TechInterviewer.Controllers.WorkIndustries.Dtos;

public record WorkIndustryDto
{
    public long Id { get; init; }

    public string Title { get; init; }

    public HexColor HexColor { private get; init; }

    public string HexColorAsString => HexColor.ToString();
}