using System;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Salaries.GetSalariesHistoricalChart;

public record GetSalariesHistoricalChartQueryParams : SalariesChartQueryParamsBase
{
    [FromQuery(Name = "from")]
    public DateTimeOffset? From { get; init; }

    [FromQuery(Name = "to")]
    public DateTimeOffset? To { get; init; }
}