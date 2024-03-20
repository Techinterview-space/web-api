using System.Collections.Generic;
using TechInterviewer.Controllers.Labels;

namespace TechInterviewer.Features.Salaries.Models;

public record SelectBoxItemsResponse
{
    public List<LabelEntityDto> Skills { get; init; }

    public List<LabelEntityDto> Industries { get; init; }

    public List<LabelEntityDto> Professions { get; init; }
}