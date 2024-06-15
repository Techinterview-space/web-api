using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Salaries.GetSalaries;

public record GetSalariesPaginatedQuery
    : PageModel, IRequest<Pageable<UserSalaryDto>>, ISalariesChartQueryParams
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