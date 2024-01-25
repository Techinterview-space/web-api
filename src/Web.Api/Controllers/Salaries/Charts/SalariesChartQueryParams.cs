using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Microsoft.AspNetCore.Mvc;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record SalariesChartQueryParams
{
    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }

    [FromQuery(Name = "prof_include")]
    public List<UserProfession> ProfessionsToInclude { get; init; } = new ();

    [FromQuery(Name = "prof_exclude")]
    public List<UserProfession> ProfessionsToExclude { get; init; } = new ();
}