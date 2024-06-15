using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record DevelopersByGradeDistributionData
{
    public int All { get; }

    public List<ResponseItem> Items { get; }

    public DevelopersByGradeDistributionData(
        List<UserSalaryDto> salaries)
    {
        All = salaries.Count;
        var grades = salaries
            .Select(x => x.Grade)
            .Distinct()
            .ToList();

        Items = new List<ResponseItem>();
        foreach (var grade in grades)
        {
            Items.Add(new ResponseItem
            {
                Grade = grade,
                Count = salaries.Count(x => x.Grade == grade),
            });
        }
    }

    public record ResponseItem
    {
        public DeveloperGrade? Grade { get; init; }

        public int Count { get; init; }
    }
}