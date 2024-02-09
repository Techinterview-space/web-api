using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record DevelopersByGradeDistributionData
{
    public int All { get; }

    public List<(DeveloperGrade? Grade, int Count)> Items { get; }

    public DevelopersByGradeDistributionData(
        List<UserSalaryDto> salaries)
    {
        All = salaries.Count;
        var grades = salaries
            .Select(x => x.Grade)
            .Distinct()
            .ToList();

        Items = new List<(DeveloperGrade? Grade, int Count)>();
        foreach (var grade in grades)
        {
            Items.Add((grade, salaries.Count(x => x.Grade == grade)));
        }
    }
}