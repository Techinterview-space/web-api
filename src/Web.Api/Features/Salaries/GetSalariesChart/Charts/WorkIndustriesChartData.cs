using System.Collections.Generic;
using System.Linq;
using Infrastructure.Salaries;
using Web.Api.Features.Labels.Models;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record WorkIndustriesChartData
{
    public List<WorkIndustriesChartDataItem> Items { get; }
    
    public int NoDataCount { get; }

    public WorkIndustriesChartData(
        List<UserSalaryDto> salaries, 
        List<LabelEntityDto> industries)
    {
        var salariesWithIndustries = salaries
            .Where(x => x.WorkIndustryId.HasValue)
            .ToList();

        var industryGroups = salariesWithIndustries
            .GroupBy(x => x.WorkIndustryId.Value)
            .ToList();

        Items = industryGroups
            .Select(group =>
            {
                var industry = industries.FirstOrDefault(i => i.Id == group.Key);
                return industry != null 
                    ? new WorkIndustriesChartDataItem
                    {
                        Industry = industry,
                        Count = group.Count()
                    } 
                    : null;
            })
            .Where(item => item != null)
            .OrderByDescending(item => item.Count)
            .ToList();

        NoDataCount = salaries.Count - salariesWithIndustries.Count;
    }

    public record WorkIndustriesChartDataItem
    {
        public LabelEntityDto Industry { get; init; }
        
        public int Count { get; init; }
    }
}