using System.Collections.Generic;
using System.Linq;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Microsoft.EntityFrameworkCore;
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
            .GroupBy(x => x.SkillId.Value)
            .ToList();

        Items = skillGroups
            .Select(group =>
            {
                var skill = skills.FirstOrDefault(s => s.Id == group.Key);
                return skill != null 
                    ? new SalariesSkillsChartDataItem
                    {
                        Skill = skill,
                        Count = group.Count()
                    } 
                    : null;
            })
            .Where(item => item != null)
            .OrderByDescending(item => item.Count)
            .ToList();

        NoDataCount = salaries.Count - salariesWithSkills.Count;
    }

    public record SalariesSkillsChartDataItem
    {
        public LabelEntityDto Skill { get; init; }
        
        public int Count { get; init; }
    }
}