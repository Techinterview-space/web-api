using System.Collections.Generic;
using TechInterviewer.Controllers.Skills.Dtos;
using TechInterviewer.Controllers.WorkIndustries.Dtos;

namespace TechInterviewer.Controllers.Salaries;

public record SelectBoxItemsResponse
{
    public List<SkillDto> Skills { get; init; }

    public List<WorkIndustryDto> Industries { get; init; }
}