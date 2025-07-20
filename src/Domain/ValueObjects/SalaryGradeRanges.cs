using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Entities.StatData.Salary;
using Domain.Extensions;

namespace Domain.ValueObjects;

public record SalaryGradeRanges
{
    private static readonly List<DeveloperGrade> _gradesToBeUsedInCalculation = new ()
    {
        DeveloperGrade.Lead,
        DeveloperGrade.Senior,
        DeveloperGrade.Middle,
        DeveloperGrade.Junior,
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
        foreach (var grade in _gradesToBeUsedInCalculation)
        {
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

            var higherGrade = grade.Next();
            var higherGradeOrNull = _developerGradeValues.Count > 0 && _developerGradeValues.ContainsKey(higherGrade)
                ? _developerGradeValues[higherGrade]
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

    public SalaryGradeRangesItem InWhatRangeValueIs(
        double? min,
        double? max)
    {
        if (!min.HasValue && !max.HasValue)
        {
            throw new InvalidOperationException("Min and max cannot be null.");
        }

        if (max.HasValue && !min.HasValue)
        {
            return InWhatRangeValueIs(max.Value);
        }

        if (!max.HasValue)
        {
            return InWhatRangeValueIs(min.Value);
        }

        // If both min and max are provided, we need to find the range that contains both values.
        throw new NotSupportedException("Min and max cannot be null.");
    }

    private SalaryGradeRangesItem InWhatRangeValueIs(
        double value)
    {
        var item = _developerGradeValues
            .FirstOrDefault(x => x.Value.IsInRange(value)).Value;

        return item;
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