using System.Collections.Generic;
using System.Linq;
using Domain.Extensions;
using Domain.ValueObjects.Ranges;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public class SalariesByUserAgeChart
{
    public List<IntRange> Labels { get; }

    public List<int> ItemsCount { get; }

    public List<double> MedianSalaries { get; }

    public List<double> AverageSalaries { get; }

    public SalariesByUserAgeChart(
        List<UserSalaryDto> salaries)
    {
        Labels = new List<IntRange>();
        MedianSalaries = new List<double>();
        AverageSalaries = new List<double>();
        ItemsCount = new List<int>();

        var salariesWithExperience = salaries
            .Where(x => x.Age.HasValue)
            .Select(x => new
            {
                x.Value,
                Age = x.Age.Value
            })
            .ToList();

        var splitter = new PeopleAgeValuesSplitter();
        var ranges = splitter.ToList();

        for (var index = 0; index < ranges.Count; index++)
        {
            var range = ranges[index];
            var items = salariesWithExperience
                .Where(x =>
                    x.Age >= range.Start &&
                    (x.Age < range.End || (index == ranges.Count - 1 && x.Age >= range.End)))
                .ToList();

            var median = items.Count > 0 ? items.Median(x => x.Value) : 0;
            var average = items.Count > 0 ? items.Average(x => x.Value) : 0;

            Labels.Add(new IntRange((int)range.Start, (int)range.End));
            MedianSalaries.Add(median);
            AverageSalaries.Add(average);
            ItemsCount.Add(items.Count);
        }
    }
}