using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record ProfessionsDistributionData
{
    public int All { get; }

    public List<(UserProfession Profession, int Count)> Items { get; }

    public ProfessionsDistributionData(
        List<UserSalaryDto> salaries)
    {
        All = salaries.Count;
        Items = salaries
            .GroupBy(x => x.Profession)
            .Select(x => (x.Key, x.Count()))
            .ToList();
    }
}