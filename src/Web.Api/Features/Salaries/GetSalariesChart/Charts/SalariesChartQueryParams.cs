﻿using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Enums;
using Infrastructure.Salaries;
using Microsoft.AspNetCore.Mvc;

namespace TechInterviewer.Features.Salaries.GetSalariesChart.Charts;

// TODO to be removed
public record SalariesChartQueryParams : ISalariesChartQueryParams
{
    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }

    [FromQuery(Name = "profsInclude")]
    public List<long> ProfessionsToInclude { get; init; } = new ();

    [FromQuery(Name = "cities")]
    public List<KazakhstanCity> Cities { get; init; } = new ();

    public bool HasAnyFilter =>
        Grade.HasValue || ProfessionsToInclude.Count > 0 || Cities.Count > 0;
}