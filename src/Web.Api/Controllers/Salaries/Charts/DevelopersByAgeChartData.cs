﻿using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Services;
using Domain.Services.Salaries;
using Domain.ValueObjects.Ranges;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record DevelopersByAgeChartData
{
    public List<DoublesRange> Labels { get; }

    public List<int> Data { get; }

    public DevelopersByAgeChartData(
        List<UserSalaryDto> salaries)
    {
        Labels = new List<DoublesRange>();
        Data = new List<int>();

        var salariesWithAge = salaries
            .Where(x => x.Age.HasValue)
            .Select(x => x.Age.Value)
            .ToList();

        var splitter = new ValuesByRangesSplitter(15, 55, 5);
        var ranges = splitter.ToList();

        for (var index = 0; index < ranges.Count; index++)
        {
            var range = ranges[index];
            var count = salariesWithAge
                .Count(x =>
                    x >= range.Start &&
                    (x < range.End || (index == ranges.Count - 1 && x >= range.End)));

            Labels.Add(range);
            Data.Add(count);
        }
    }
}