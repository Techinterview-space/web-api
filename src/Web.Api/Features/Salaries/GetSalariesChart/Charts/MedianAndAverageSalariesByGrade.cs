using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Extensions;
using Infrastructure.Salaries;

namespace TechInterviewer.Features.Salaries.GetSalariesChart.Charts;

public record MedianAndAverageSalariesByGrade
{
    public MedianAndAverageSalariesByGrade(
        DeveloperGrade grade,
        List<UserSalaryDto> salaries)
    {
        Grade = grade;
        AverageSalary = salaries.Count > 0 ? salaries.Select(x => x.Value).Average() : null;
        MedianSalary = salaries.Count > 0 ? salaries.Select(x => x.Value).Median() : null;
    }

    public DeveloperGrade Grade { get; }

    public double? AverageSalary { get; }

    public double? MedianSalary { get; }
}