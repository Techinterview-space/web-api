using System;
using Domain.ValueObjects;

namespace TechInterviewer.Features.Salaries.GetSalariesHistoricalChart;

public record WeekSplitter : DateTimeRangeSplitter
{
    public WeekSplitter(
        DateTime min,
        DateTime max)
        : base(min, max, TimeSpan.FromDays(7))
    {
    }
}