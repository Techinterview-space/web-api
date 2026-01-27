using System.Collections.Generic;
using System.Linq;
using Infrastructure.Salaries;
using Web.Api.Features.Labels.Models;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record SalariesSkillsChartData
{
    public List<SalariesSkillsChartDataItem> Items { get; }

    public int NoDataCount { get; }

    public SalariesSkillsChartData(
        List<UserSalaryDto> salaries,
        List<LabelEntityDto> skills)
    {
        var salariesWithSkills = salaries
            .Where(x => x.SkillId.HasValue)
            .ToList();

        var skillGroups = salariesWithSkills
            .GroupBy(x => x.SkillId.GetValueOrDefault())
            .ToList();

        Items = skillGroups
            .Select(group =>
            {
                var skill = skills.FirstOrDefault(s => s.Id == group.Key);
                return skill != null
                    ? new SalariesSkillsChartDataItem(
                        skill: skill,
                        count: group.Count())
                    : null;
            })
            .Where(item => item != null)
            .OrderByDescending(item => item.Count)
            .ToList();

        NoDataCount = salaries.Count - salariesWithSkills.Count;
    }

    public record SalariesSkillsChartDataItem
    {
        public SalariesSkillsChartDataItem(
            LabelEntityDto skill,
            int count)
        {
            Skill = skill;
            Count = count;
        }

        public LabelEntityDto Skill { get; }

        public int Count { get; }
    }
}