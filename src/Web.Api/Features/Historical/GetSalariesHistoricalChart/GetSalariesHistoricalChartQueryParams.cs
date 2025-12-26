using System;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Historical.GetSalariesHistoricalChart;

public record GetSalariesHistoricalChartQueryParams
{
    [FromQuery(Name = "from")]
    public DateTimeOffset? From { get; init; }

    [FromQuery(Name = "to")]
    public DateTimeOffset? To { get; init; }
}