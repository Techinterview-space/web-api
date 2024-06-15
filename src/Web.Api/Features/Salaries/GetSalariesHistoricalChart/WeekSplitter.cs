using System;
using Domain.ValueObjects;

namespace Web.Api.Features.Salaries.GetSalariesHistoricalChart;

public record WeekSplitter : DateTimeRangeSplitter
{
    public WeekSplitter(
        DateTime min,
        DateTime max)
        : base(min, max, TimeSpan.FromDays(7))
    {
    }
}