using System.Collections.Generic;
using System.Linq;
using Domain.ValueObjects;
using Domain.ValueObjects.Ranges;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record DevelopersByExperienceYears
{
    private const int DefaultMaxExperience = 65;

    public List<IntRange> Labels { get; }

    public List<int> Data { get; }

    public DevelopersByExperienceYears(
        List<UserSalaryDto> salaries)
    {
        Labels = new List<IntRange>();
        Data = new List<int>();

        var salariesWithExperience = salaries
            .Where(x => x.YearsOfExperience.HasValue)
            .Select(x => x.YearsOfExperience.Value)
            .ToList();

        var splitter = new ValuesByRangesSplitter(0, 15, 1);
        var ranges = splitter.ToList();

        for (var index = 0; index < ranges.Count; index++)
        {
            var range = ranges[index];
            var count = salariesWithExperience
                .Count(x =>
                    x >= range.Start &&
                    (x < range.End || (index == ranges.Count - 1 && x >= range.End)));

            Labels.Add(new IntRange((int)range.Start, (int)range.End));
            Data.Add(count);
        }
    }
}