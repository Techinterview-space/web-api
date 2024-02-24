using Domain.ValueObjects;

namespace TechInterviewer.Controllers.Labels;

public record LabelEntityDto
{
    public long Id { get; init; }

    public string Title { get; init; }

    public HexColor HexColor { private get; init; }

    public string HexColorAsString => HexColor.ToString();
}