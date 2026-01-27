using System;
using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Salaries;

public record SalariesChartQueryParamsBase : ISalariesChartQueryParams
{
    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }

    [FromQuery(Name = "profsInclude")]
    public List<long> SelectedProfessionIds { get; init; } = new ();

    [FromQuery(Name = "cities")]
    public List<KazakhstanCity> Cities { get; init; } = new ();

    [FromQuery(Name = "skills")]
    public List<long> Skills { get; init; } = new ();

    [FromQuery(Name = "salarySourceTypes")]
    public List<SalarySourceType> SalarySourceTypes { get; init; } = new ();

    [FromQuery(Name = "quarterTo")]
    public int? QuarterTo { get; init; }

    [FromQuery(Name = "yearTo")]
    public int? YearTo { get; init; }

    [FromQuery(Name = "dateTo")]
    public DateTime? DateTo { get; init; }

    public Domain.ValueObjects.SalariesChartQueryParamsBase CreateDatabaseQueryParams()
    {
        return new Domain.ValueObjects.SalariesChartQueryParamsBase(
            Grade,
            SelectedProfessionIds,
            Skills,
            Cities,
            SalarySourceTypes,
            QuarterTo,
            YearTo,
            DateTo);
    }
}