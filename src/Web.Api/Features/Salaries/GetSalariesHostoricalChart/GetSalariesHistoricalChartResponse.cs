using System;
using System.Collections.Generic;
using Domain.Entities.Enums;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Salaries;

namespace TechInterviewer.Features.Salaries.GetSalariesHostoricalChart;

public record GetSalariesHistoricalChartResponse
{
    private static readonly List<DeveloperGrade> _gradesToBeUsedInChart = new ()
    {
        DeveloperGrade.Junior,
        DeveloperGrade.Middle,
        DeveloperGrade.Senior,
        DeveloperGrade.Lead,
    };

    public GetSalariesHistoricalChartResponse()
    {
    }

    public GetSalariesHistoricalChartResponse(
        List<UserSalarySimpleDto> salaries,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        Salaries = salaries;
        RangeStart = from;
        RangeEnd = to;
        HasAuthentication = true;
        ShouldAddOwnSalary = false;
    }

    public List<UserSalarySimpleDto> Salaries { get; private set; }

    public bool ShouldAddOwnSalary { get; private set; }

    public bool HasAuthentication { get; private set; }

    public DateTimeOffset RangeStart { get; private set; }

    public DateTimeOffset RangeEnd { get; private set; }

    public List<CurrencyContent> Currencies { get; private set; }

    public static GetSalariesHistoricalChartResponse NoSalaryOrAuthorization(
        bool hasAuthentication,
        bool shouldAddOwnSalary,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd)
    {
        return new GetSalariesHistoricalChartResponse
        {
            HasAuthentication = hasAuthentication,
            ShouldAddOwnSalary = shouldAddOwnSalary,
            Currencies = new List<CurrencyContent>(),
            RangeStart = rangeStart,
            RangeEnd = rangeEnd,
        };
    }
}