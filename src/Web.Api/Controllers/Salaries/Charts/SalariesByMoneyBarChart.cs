using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;
using Domain.Services;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.Charts;

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

        ItemsByProfession = salaries
            .GroupBy(x => x.Profession)
            .Select(x => new ItemsByProfessionByValuePeriods(
                x.Key,
                splitter
                    .Select(y => new ItemsByValuePeriods(
                        y.Start,
                        y.End,
                        x.Count(z => z.Value >= y.Start && z.Value < y.End)))
                    .ToList()))
            .ToList();

        Step = StepValue;
    }

    public List<string> Labels { get; }

    public List<ItemsByValuePeriods> Items { get; }

    public List<ItemsByProfessionByValuePeriods> ItemsByProfession { get; }

    public int Step { get; }

#pragma warning disable SA1313
    public record ItemsByProfessionByValuePeriods(
        UserProfession Profession,
        List<ItemsByValuePeriods> Items);

    public record ItemsByValuePeriods(
        double Start,
        double End,
        int Count);
#pragma warning restore SA1313
}