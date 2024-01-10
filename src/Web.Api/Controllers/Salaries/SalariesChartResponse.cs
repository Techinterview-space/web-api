using System;
using System.Collections.Generic;
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
    }

    public static SalariesChartResponse RequireOwnSalary()
    {
        return new(
            new List<UserSalaryDto>(),
            true,
            null,
            null);
    }

    public List<UserSalaryDto> Salaries { get; }

    public bool ShouldAddOwnSalary { get; }

    public DateTimeOffset? RangeStart { get; }

    public DateTimeOffset? RangeEnd { get; }
}