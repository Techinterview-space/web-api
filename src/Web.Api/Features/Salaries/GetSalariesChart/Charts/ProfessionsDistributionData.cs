using System.Collections.Generic;
using System.Linq;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record ProfessionsDistributionData
{
    public int All { get; }

    public List<ProfessionsDistributionDataItem> Items { get; }

    public ProfessionsDistributionData(
        List<UserSalaryDto> salaries)
    {
        All = salaries.Count;
        Items = salaries
            .GroupBy(x => x.ProfessionId)
            .Select(x => new ProfessionsDistributionDataItem
            {
                Profession = x.Key.GetValueOrDefault(),
                Count = x.Count(),
            })
            .ToList();
    }

    public record ProfessionsDistributionDataItem
    {
        public long Profession { get; init; }

        public int Count { get; init; }
    }
}