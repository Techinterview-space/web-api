using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
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

    private readonly int _salariesCount;
    private readonly Dictionary<DeveloperGrade, List<SalaryBaseData>> _salaries;
    private readonly Dictionary<DeveloperGrade, SalaryGradeRangesItem> _developerGradeValues;
    private readonly List<Profession> _professions;

    public SalaryGradeRanges(
        List<SalaryBaseData> salaries,
        List<Profession> professionses)
    {
        if (salaries == null)
        {
            throw new InvalidOperationException("Salaries cannot be null.");
        }

        _salariesCount = salaries.Count;
        _professions = professionses;

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
                        null,
                        null));

                continue;
            }

            var higherGrade = gradeGroup.Next().ToDeveloperGrade();
            var higherGradeOrNull = _developerGradeValues.Count > 0 && _developerGradeValues.TryGetValue(higherGrade, out var value)
                ? value
                : null;

            var min = gradeSalaries.First().Value;
            var median = gradeSalaries.Select(x => x.Value).Median();
            var max = gradeSalaries.Last().Value;

            _developerGradeValues.Add(
                grade,
                new SalaryGradeRangesItem(
                    grade,
                    min,
                    max,
                    median));
        }
    }

    public string ToTelegramHtml(
        double? min,
        double? max)
    {
        var developerGrades = InWhatRangeValueIs(min, max);
        if (developerGrades.Count == 0)
        {
            return null;
        }

        var professionPostfix = string.Empty;
        if (_professions != null && _professions.Count > 0)
        {
            professionPostfix = _professions.Count == 1
                ? $" ({_professions[0].Title})"
                : $" ({string.Join(", ", _professions.Select(x => x.Title))})";
        }

        string text;
        if (developerGrades.Count == 1)
        {
            text = $"💰Указанная зарплата соответствует уровню <b>{developerGrades[0]}</b>{professionPostfix}.\n\n";
        }
        else
        {
            text = $"💰Указанная зарплата соответствует уровням {string.Join(", ", developerGrades.Select(x => $"<b>{x}</b>"))}{professionPostfix}. " +
                   $"По среднему значению ближе к уровню <b>{developerGrades[1]}</b>.\n\n";
        }

        text += "📈<b>Диапазон:</b> ";
        if (min.HasValue && max.HasValue)
        {
            text += $"{new SalarySpaceFormattedValue(min.Value)} - {new SalarySpaceFormattedValue(max.Value)} ₸\n";
        }
        else if (min.HasValue)
        {
            text += $"от {new SalarySpaceFormattedValue(min.Value)} ₸\n";
        }
        else if (max.HasValue)
        {
            text += $"до {new SalarySpaceFormattedValue(max.Value)} ₸\n";
        }

        text += $"На основе {_salariesCount} анкет.";

        // TODO mgorbatyuk: add utm_source link there
        text += "\n\n<em>Источник techinterview.space</em>";
        return text;
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
        var closestGradeByMedianData = _developerGradeValues
            .Where(x => x.Value.Median.HasValue)
            .Select(x => new
            {
                x.Value.Median,
                RelativeDifference = x.Value.GetDifferenceWithMedian(value, false),
                AbsoluteDifference = x.Value.GetDifferenceWithMedian(value, true),
                x.Value.Grade
            })
            .Where(x => x.AbsoluteDifference.HasValue)
            .OrderBy(x => x.AbsoluteDifference.Value)
            .ToList();

        if (closestGradeByMedianData.Count > 0)
        {
            grade = closestGradeByMedianData[0].Grade;
            return true;
        }

        var matches = _developerGradeValues
            .Where(x => x.Value.IsInRange(value))
            .ToList();

        if (matches.Count > 0)
        {
            grade = matches[0].Value.Grade;
            return true;
        }

        grade = default;
        return false;
    }
}

public record SalaryGradeRangesItem
{
    public SalaryGradeRangesItem(
        DeveloperGrade grade,
        double? min,
        double? max,
        double? median)
    {
        Grade = grade;
        Min = min;
        Max = max;
        Median = median;
    }

    public DeveloperGrade Grade { get; }

    public double? Min { get; }

    public double? Max { get; }

    public double? Median { get; }

    public bool IsInRange(
        double value)
    {
        if (Min.HasValue && Max.HasValue)
        {
            return
                (value >= Min.Value && value <= Max.Value) ||
                (Grade is DeveloperGrade.Junior && value < Min.Value) ||
                (Grade is DeveloperGrade.Lead && value > Median);
        }

        if (Min.HasValue)
        {
            return
                value >= Min.Value ||
                (Grade is DeveloperGrade.Junior && value < Min.Value);
        }

        return
            value <= Max ||
            (Grade is DeveloperGrade.Lead && value > Median);
    }

    public double? GetDifferenceWithMedian(
        double value,
        bool absolute)
    {
        if (Median == null)
        {
            return null;
        }

        return absolute
            ? Math.Abs(Median.Value - value)
            : Median.Value - value;
    }
}