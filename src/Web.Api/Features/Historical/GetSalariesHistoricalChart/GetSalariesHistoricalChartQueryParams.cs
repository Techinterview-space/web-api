using System;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Salaries;

namespace Web.Api.Features.Historical.GetSalariesHistoricalChart;

public record GetSalariesHistoricalChartQueryParams : SalariesChartQueryParamsBase
{
    [FromQuery(Name = "from")]
    public DateTimeOffset? From { get; init; }

    [FromQuery(Name = "to")]
    public DateTimeOffset? To { get; init; }
}