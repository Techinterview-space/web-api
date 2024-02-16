using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record PeopleByGradesChartData
{
    public int AllCount { get; }

    public List<Item> Data { get; }

    public PeopleByGradesChartData(
        List<UserSalaryDto> salaries)
    {
        AllCount = salaries.Count;
        Data = salaries
            .Select(x => x.Grade ?? DeveloperGrade.Unknown)
            .GroupBy(x => x)
            .Select(x => new Item
            {
                Grade = x.Key,
                Count = x.Count(),
            })
            .ToList();
    }

    public record Item
    {
        public DeveloperGrade Grade { get; init; }

        public int Count { get; init; }
    }
}