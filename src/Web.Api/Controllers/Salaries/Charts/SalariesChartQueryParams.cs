using Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record SalariesChartQueryParams
{
    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }
}