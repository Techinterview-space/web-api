using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TechInterviewer.Features.Salaries.GetAdminChart;

public record GetAddingTrendChartQuery : IRequest<GetAddingTrendChartResponse>
{
    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }

    [FromQuery(Name = "profsInclude")]
    public List<long> ProfessionsToInclude { get; init; } = new ();

    [FromQuery(Name = "cities")]
    public List<KazakhstanCity> Cities { get; init; } = new ();
}