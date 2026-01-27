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
            .GroupBy(x => x.WorkIndustryId.GetValueOrDefault())
            .ToList();

        Items = industryGroups
            .Select(group =>
            {
                var industry = industries.FirstOrDefault(i => i.Id == group.Key);
                return industry != null
                    ? new WorkIndustriesChartDataItem(
                        industry: industry,
                        count: group.Count())
                    : null;
            })
            .Where(item => item != null)
            .OrderByDescending(item => item.Count)
            .ToList();

        NoDataCount = salaries.Count - salariesWithIndustries.Count;
    }

    public record WorkIndustriesChartDataItem
    {
        public WorkIndustriesChartDataItem(
            LabelEntityDto industry,
            int count)
        {
            Industry = industry;
            Count = count;
        }

        public LabelEntityDto Industry { get; }

        public int Count { get; }
    }
}