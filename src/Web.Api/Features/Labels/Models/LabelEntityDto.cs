using Domain.ValueObjects;

namespace TechInterviewer.Features.Labels.Models;

public record LabelEntityDto
{
    public long Id { get; init; }

    public string Title { get; init; }

    public HexColor HexColor { private get; init; }

    public string HexColorAsString => HexColor.ToString();
}