using System;
using System.Collections.Generic;

namespace TechInterviewer.Features.Salaries.GetAdminChart;

public record GetAddingTrendChartResponse
{
    public const string DateTimeFormat = "dd:MM";

    public List<string> Labels { get; init; } = new ();

    public List<AdminChartItem> Items { get; init; } = new ();

#pragma warning disable SA1313
    public record AdminChartItem(
        int Count,
        DateTime StartedAt);
}