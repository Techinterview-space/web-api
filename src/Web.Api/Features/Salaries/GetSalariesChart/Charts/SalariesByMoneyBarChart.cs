using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ValueObjects;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record SalariesByMoneyBarChart
{
    public const int StepValue = 250_000;

    public SalariesByMoneyBarChart(
        List<UserSalaryDto> salaries)
    {
        var values = salaries
            .Select(x => x.Value)
            .ToList();

        var minSalary = values.Min();
        var maxSalary = values.Max();

        var splitter = new RoundedValuesByRangesSplitter(minSalary, maxSalary, StepValue).ToList();
        Labels = splitter
            .Select(x => x.End.ToString("000"))
            .ToList();

        Items = splitter
            .Select(x =>
                values.Count(y =>
                    y >= x.Start &&
                    (y < x.End || (Math.Abs(x.End - maxSalary) < 0.01 && Math.Abs(y - maxSalary) < 0.01))))
            .ToList();

        ItemsByProfession = salaries
            .GroupBy(x => x.ProfessionId)
            .Select(x => new ItemsByProfessionByValuePeriods(
                x.Key.GetValueOrDefault(),
                x.Key,
                splitter
                    .Select(y =>
                        x.Count(z =>
                            z.Value >= y.Start &&
                            (z.Value < y.End || (Math.Abs(y.End - maxSalary) < 0.01 && Math.Abs(z.Value - maxSalary) < 0.01))))
                    .ToList()))
            .ToList();

        Step = StepValue;
    }

    public List<string> Labels { get; }

    public List<int> Items { get; }

    public List<ItemsByProfessionByValuePeriods> ItemsByProfession { get; }

    public int Step { get; }

#pragma warning disable SA1313
    public record ItemsByProfessionByValuePeriods(
        long Profession,
        long? ProfessionId,
        List<int> Items);
#pragma warning restore SA1313
}