using System.Collections.Generic;
using System.Linq;
using Domain.Extensions;
using Domain.ValueObjects.Ranges;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record SalariesByExperienceChart
{
    public List<IntRange> Labels { get; }

    public List<int> ItemsCount { get; }

    public List<double> MedianSalaries { get; }

    public List<double> AverageSalaries { get; }

    public SalariesByExperienceChart(
        List<UserSalaryDto> salaries)
    {
        Labels = new List<IntRange>();
        MedianSalaries = new List<double>();
        AverageSalaries = new List<double>();
        ItemsCount = new List<int>();

        var salariesWithExperience = salaries
            .Where(x => x.YearsOfExperience.HasValue)
            .Select(x => new
            {
                x.Value,
                YearsOfExperience = x.YearsOfExperience.Value
            })
            .ToList();

        var splitter = new UserExperienceValuesSplitter();
        var ranges = splitter.ToList();

        for (var index = 0; index < ranges.Count; index++)
        {
            var range = ranges[index];
            var items = salariesWithExperience
                .Where(x =>
                    x.YearsOfExperience >= range.Start &&
                    (x.YearsOfExperience < range.End || (index == ranges.Count - 1 && x.YearsOfExperience >= range.End)))
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