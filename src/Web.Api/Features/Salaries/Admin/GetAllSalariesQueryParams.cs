using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.ValueObjects.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Salaries.Admin;

public record GetAllSalariesQueryParams : PageModel
{
    [FromQuery(Name = "company")]
    public CompanyType? CompanyType { get; init; }

    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }

    [FromQuery(Name = "profession")]
    public long? Profession { get; init; }

    [FromQuery(Name = "order_type")]
    public GetAllSalariesOrderType OrderType { get; init; } = GetAllSalariesOrderType.CreatedAtDesc;
}