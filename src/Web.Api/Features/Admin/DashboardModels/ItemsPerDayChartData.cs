using System;
using System.Collections.Generic;
using System.Linq;

namespace Web.Api.Features.Admin.DashboardModels;

public record ItemsPerDayChartData
{
    public List<string> Labels { get; }

    public List<int> Items { get; }

    public ItemsPerDayChartData(
        List<DateTimeOffset> sourceItems)
    {
        Labels = new List<string>();
        Items = new List<int>();

        var itemsGroupedByDay = sourceItems
            .GroupBy(x => x.Date.Date)
            .Select(x => new
            {
                Date = x.Key,
                Label = x.Key.ToString("yyyy-MM-dd"),
                Count = x.Count(),
            })
            .ToList();

        foreach (var item in itemsGroupedByDay)
        {
            Labels.Add(item.Label);
            Items.Add(item.Count);
        }
    }
}