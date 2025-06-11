using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ValueObjects;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record SalariesByMoneyBarChart
{
    public const int StepValue = 250_000;
    public const double Toleration = 0.01;
    public const double ItemsRateToCut = 0.05;

    public SalariesByMoneyBarChart(
        List<double> orderedSalaryValues)
    {
        var removeCount = (int)Math.Floor(orderedSalaryValues.Count * ItemsRateToCut);
        var salariesToBeProcessed = orderedSalaryValues
            .Skip(removeCount)
            .Take(orderedSalaryValues.Count - (removeCount * 2))
            .ToList();

        var minSalary = salariesToBeProcessed.Min();
        var maxSalary = salariesToBeProcessed.Max();

        var splitter = new RoundedValuesByRangesSplitter(minSalary, maxSalary, StepValue).ToList();
        Labels = splitter
            .Select(x => x.End.ToString("000"))
            .ToList();

        Items = splitter
            .Select(x =>
                salariesToBeProcessed.Count(y =>
                    y >= x.Start &&
                    (y < x.End || (Math.Abs(x.End - maxSalary) < Toleration && Math.Abs(y - maxSalary) < Toleration))))
            .ToList();

        Step = StepValue;
    }

    public List<string> Labels { get; }

    public List<int> Items { get; }

    public int Step { get; }
}