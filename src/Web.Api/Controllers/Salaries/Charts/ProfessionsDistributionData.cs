using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.Charts;

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