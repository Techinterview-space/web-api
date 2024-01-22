﻿using System;
using System.Collections.Generic;

namespace TechInterviewer.Controllers.Salaries.AdminChart;

public record AdminChartResponse
{
    public const string DateTimeFormat = "dd HH:mm";

    public List<string> Labels { get; init; } = new ();

    public List<AdminChartItem> Items { get; init; } = new ();

    public double SalariesPerUser { get; init; }

#pragma warning disable SA1313
    public record AdminChartItem(
        int Count,
        DateTime StartedAt);
}