using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

// TODO to be removed
public record SalariesChartQueryParams : ISalariesChartQueryParams
{
    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }

    [FromQuery(Name = "profsInclude")]
    public List<long> SelectedProfessionIds { get; init; } = new ();

    [FromQuery(Name = "skills")]
    public List<long> Skills { get; init; } = new ();

    [FromQuery(Name = "cities")]
    public List<KazakhstanCity> Cities { get; init; } = new ();

    [FromQuery(Name = "salarySourceTypes")]
    public List<SalarySourceType> SalarySourceTypes { get; init; } = new ();

    [FromQuery(Name = "quarterTo")]
    public int? QuarterTo { get; init; }

    [FromQuery(Name = "yearTo")]
    public int? YearTo { get; init; }

    public bool HasAnyFilter =>
        Grade.HasValue || SelectedProfessionIds.Count > 0 || Cities.Count > 0;

    public string GetKeyPostfix()
    {
        var grade = Grade?.ToString() ?? "all";
        var professions = SelectedProfessionIds.Count == 0 ? "all" : string.Join("_", SelectedProfessionIds);
        return $"{grade}_{professions}";
    }
}