using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using Domain.Extensions;

namespace Domain.Entities.StatData.Salary;

// mgorbatyuk: old props should not be renamed.
public record SalariesStatDataCacheItemSalaryData
{
    public static readonly List<GradeGroup> GradeGroupsForRegularStats = EnumHelper
        .Values<GradeGroup>()
        .Where(x => x is not(GradeGroup.Undefined or GradeGroup.Trainee))
        .ToList();

    public SalariesStatDataCacheItemSalaryData()
    {
    }

    public SalariesStatDataCacheItemSalaryData(
        List<SalaryBaseData> values,
        int totalCount)
    {
        var salaryValues = values
            .Select(x => x.Value)
            .ToList();

        MedianLocalSalary = salaryValues.Median();
        AverageLocalSalary = salaryValues.Count > 0 ? salaryValues.Average() : 0;
        TotalSalaryCount = totalCount;
        MinLocalSalary = salaryValues.Count > 0 ? salaryValues.Min() : null;
        MaxLocalSalary = salaryValues.Count > 0 ? salaryValues.Max() : null;

        MedianLocalSalaryByGrade = new Dictionary<GradeGroup, double>();

        foreach (var gradeGroup in GradeGroupsForRegularStats)
        {
            var gradeValues = values
                .Where(x => x.Grade.GetGroupNameOrNull() == gradeGroup)
                .Select(x => x.Value)
                .ToList();

            if (MedianLocalSalaryByGrade.ContainsKey(gradeGroup))
            {
                MedianLocalSalaryByGrade[gradeGroup] = gradeValues.Median();
            }
            else
            {
                MedianLocalSalaryByGrade.Add(gradeGroup, gradeValues.Median());
            }
        }
    }

    public double MedianLocalSalary { get; init; }

    public double AverageLocalSalary { get; init; }

    public double? MinLocalSalary { get; init; }

    public double? MaxLocalSalary { get; init; }

    public Dictionary<GradeGroup, double> MedianLocalSalaryByGrade { get; init; } = new ();

    public int TotalSalaryCount { get; init; }

    public double? GetMedianLocalSalaryByGrade(
        GradeGroup gradeGroup)
    {
        if (MedianLocalSalaryByGrade.TryGetValue(gradeGroup, out var value))
        {
            return value;
        }

        return null;
    }
}