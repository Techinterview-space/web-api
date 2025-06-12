using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Salaries.GetAdminChart;

public record GetAddingTrendChartQuery
{
    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }

    [FromQuery(Name = "profsInclude")]
    public List<long> ProfessionsToInclude { get; init; } = new ();

    [FromQuery(Name = "cities")]
    public List<KazakhstanCity> Cities { get; init; } = new ();

    [FromQuery(Name = "salarySourceType")]
    public SalarySourceType? SalarySourceType { get; init; }

    [FromQuery(Name = "skills")]
    public List<long> Skills { get; init; } = new ();

    [FromQuery(Name = "quarterTo")]
    public int? QuarterTo { get; init; }

    [FromQuery(Name = "yearTo")]
    public int? YearTo { get; init; }
}