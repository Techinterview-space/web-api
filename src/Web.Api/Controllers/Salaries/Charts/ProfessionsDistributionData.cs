using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record ProfessionsDistributionData
{
    public int All { get; }

    public List<Item> Items { get; }

    public ProfessionsDistributionData(
        List<UserSalaryDto> salaries)
    {
        All = salaries.Count;
        Items = salaries
            .GroupBy(x => x.Profession)
            .Select(x => new Item
            {
                Profession = x.Key,
                Count = x.Count(),
            })
            .ToList();
    }

    public record Item
    {
        public UserProfessionEnum Profession { get; init; }

        public int Count { get; init; }
    }
}