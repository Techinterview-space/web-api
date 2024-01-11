using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Extensions;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries;

public record SalariesChartResponse
{
    public SalariesChartResponse(
        List<UserSalaryDto> salaries,
        DateTimeOffset? rangeStart,
        DateTimeOffset? rangeEnd)
        : this(
            salaries,
            false,
            rangeStart,
            rangeEnd)
    {
    }

    private SalariesChartResponse(
        List<UserSalaryDto> salaries,
        bool shouldAddOwnSalary,
        DateTimeOffset? rangeStart,
        DateTimeOffset? rangeEnd)
    {
        Salaries = salaries;
        ShouldAddOwnSalary = shouldAddOwnSalary;
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;

        if (salaries.Any())
        {
            AverageSalary = salaries.Select(x => x.Value).Average();
            MedianSalary = salaries.Select(x => x.Value).Median();
            SalariesByProfession = salaries
                .GroupBy(x => x.Profession)
                .Select(x => new SalariesByProfession(x.Key, x.ToList()))
                .ToList();
        }
    }

    public static SalariesChartResponse RequireOwnSalary()
    {
        return new (
            new List<UserSalaryDto>(),
            true,
            null,
            null);
    }

    public List<UserSalaryDto> Salaries { get; }

    public bool ShouldAddOwnSalary { get; }

    public double AverageSalary { get; }

    public double MedianSalary { get; }

    public DateTimeOffset? RangeStart { get; }

    public DateTimeOffset? RangeEnd { get; }

    public List<SalariesByProfession> SalariesByProfession { get; }
}