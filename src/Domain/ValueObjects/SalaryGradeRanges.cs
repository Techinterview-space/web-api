using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Entities.StatData.Salary;
using Domain.Enums;
using Domain.Extensions;

namespace Domain.ValueObjects;

public record SalaryGradeRanges
{
    private static readonly List<GradeGroup> _gradesToBeUsedInCalculation = new ()
    {
        GradeGroup.Lead,
        GradeGroup.Senior,
        GradeGroup.Middle,
        GradeGroup.Junior,
    };

    private readonly Dictionary<DeveloperGrade, List<SalaryBaseData>> _salaries;
    private readonly Dictionary<DeveloperGrade, SalaryGradeRangesItem> _developerGradeValues;

    public SalaryGradeRanges(
        List<SalaryBaseData> salaries)
    {
        if (salaries == null)
        {
            throw new InvalidOperationException("Salaries cannot be null.");
        }

        _salaries = salaries
            .GroupBy(x => x.Grade)
            .ToDictionary(
                x => x.Key,
                x => x
                    .OrderBy(s => s.Value)
                    .ToList());

        _developerGradeValues = new Dictionary<DeveloperGrade, SalaryGradeRangesItem>();
        foreach (var gradeGroup in _gradesToBeUsedInCalculation)
        {
            var grade = gradeGroup.ToDeveloperGrade();
            if (!_salaries.TryGetValue(grade, out var gradeSalaries))
            {
                _developerGradeValues.Add(
                    grade,
                    new SalaryGradeRangesItem(
                        grade,
                        null,
                        null));

                continue;
            }

            var higherGrade = gradeGroup.Next().ToDeveloperGrade();
            var higherGradeOrNull = _developerGradeValues.Count > 0 && _developerGradeValues.TryGetValue(higherGrade, out var value)
                ? value
                : null;

            var min = gradeSalaries.First().Value;
            var max = (higherGradeOrNull?.Min - 1) ?? gradeSalaries.Last().Value;

            _developerGradeValues.Add(
                grade,
                new SalaryGradeRangesItem(
                    grade,
                    min,
                    max));
        }
    }

    public List<DeveloperGrade> InWhatRangeValueIs(
        double? min,
        double? max)
    {
        if (!min.HasValue && !max.HasValue)
        {
            throw new InvalidOperationException("Min and max cannot be null.");
        }

        var result = new List<DeveloperGrade>();
        if (max.HasValue && !min.HasValue)
        {
            if (HasDeveloperGradeValue(max.Value, out var grade))
            {
                result.Add(grade);
            }
        }
        else if (!max.HasValue)
        {
            if (HasDeveloperGradeValue(min.Value, out var grade))
            {
                result.Add(grade);
            }
        }
        else
        {
            if (HasDeveloperGradeValue(min.Value, out var minGrade))
            {
                result.Add(minGrade);
            }

            var valueToSearch = (max.Value + min.Value) / 2;
            if (HasDeveloperGradeValue(valueToSearch, out var midGrade))
            {
                result.Add(midGrade);
            }

            if (HasDeveloperGradeValue(max.Value, out var maxGrade))
            {
                result.Add(maxGrade);
            }
        }

        return result
            .Distinct()
            .ToList();
    }

    private bool HasDeveloperGradeValue(
        double value,
        out DeveloperGrade grade)
    {
        grade = _developerGradeValues
            .FirstOrDefault(x => x.Value.IsInRange(value)).Value.Grade;

        return grade != default;
    }
}

public record SalaryGradeRangesItem
{
    public SalaryGradeRangesItem(
        DeveloperGrade grade,
        double? min,
        double? max)
    {
        Grade = grade;
        Min = min;
        Max = max;
    }

    public DeveloperGrade Grade { get; }

    public double? Min { get; }

    public double? Max { get; }

    public bool IsInRange(
        double value)
    {
        if (Min.HasValue && Max.HasValue)
        {
            return
                (value >= Min.Value && value <= Max.Value) ||
                (Grade is DeveloperGrade.Junior && value < Min.Value) ||
                (Grade is DeveloperGrade.Lead && value > Max);
        }

        if (Min.HasValue)
        {
            return
                value >= Min.Value ||
                (Grade is DeveloperGrade.Junior && value < Min.Value);
        }

        return
            value <= Max ||
            (Grade is DeveloperGrade.Lead && value > Max);
    }
}