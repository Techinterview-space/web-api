using System.Collections.Generic;
using System.Linq;
using Domain.Services;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries;

public record SalariesByMoneyBarChart
{
    public const int StepValue = 250_000;

    public SalariesByMoneyBarChart(
        List<UserSalaryDto> salaries)
    {
        var values = salaries
            .Select(x => x.Value)
            .ToList();

        var splitter = new RoundedValuesByRangesSplitter(
            values.Min(),
            values.Max(),
            StepValue)
            .ToList();

        Labels = splitter
            .Select(x => x.End.ToString("000"))
            .ToList();

        Items = splitter
            .Select(x => new ItemsByValuePeriods(
                x.Start,
                x.End,
                values.Count(y => y >= x.Start && y < x.End)))
            .ToList();

        Step = StepValue;
    }

    public List<string> Labels { get; }

    public List<ItemsByValuePeriods> Items { get; }

    public int Step { get; }

#pragma warning disable SA1313
    public record ItemsByValuePeriods(
        double Start,
        double End,
        int Count);
#pragma warning restore SA1313
}