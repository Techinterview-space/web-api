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
                ? $" по специальности <b>{_professions[0].Title}</b>"
                : $" по специальностям <b>{string.Join(", ", _professions.Select(x => x.Title))}</b>";
        }

        string text;
        if (developerGrades.Count == 1)
        {
            text = $"Зарплата в вакансии соответствует уровню <b>{developerGrades[0]}</b>{professionPostfix}.\n\n";
        }
        else
        {
            text = $"Зарплата в вакансии соответствует уровням {string.Join(", ", developerGrades.Select(x => $"<b>{x}</b>"))}{professionPostfix}, " +
                   $"но скорее всего это <b>{developerGrades[1]}</b> на основе среднего арифметического вилки.\n\n";
        }

        text += $"Данные построены на основе анализа следующих данных:\n";
        if (min.HasValue)
        {
            text += $"От <b>{min.Value:N0}</b>.\n";
        }

        if (max.HasValue)
        {
            text += $"До <b>{max.Value:N0}</b>.\n";
        }

        text += $"Кол-во анкет: <b>{_salaries.Count}</b>";

        // TODO mgorbatyuk: add utm_source link there
        text += "\n\n<em>Данные предоставлены порталом <a href=\"https://techinterview.space/salaries\">techinterview.space</a></em>";
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