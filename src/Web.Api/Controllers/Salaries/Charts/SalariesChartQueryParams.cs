using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Microsoft.AspNetCore.Mvc;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record SalariesChartQueryParams
{
    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }

    [FromQuery(Name = "profsInclude")]
    public List<UserProfession> ProfessionsToInclude { get; init; } = new ();

    [FromQuery(Name = "profsExclude")]
    public List<UserProfession> ProfessionsToExclude { get; init; } = new ();
}