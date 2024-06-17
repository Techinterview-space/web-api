using System;
using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.ValueObjects.Dates;
using Infrastructure.Salaries;
using Web.Api.Features.Historical.GetSalariesHistoricalChart.Charts;

namespace Web.Api.Features.Historical.GetSalariesHistoricalChart;

public record GetSalariesHistoricalChartResponse
{
    public static readonly List<DeveloperGrade> GradesToBeUsedInChart = new ()
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
        DateTimeOffset to,
        bool addGradeChartData)
    {
        From = from;
        To = to;
        HasAuthentication = true;
        ShouldAddOwnSalary = false;

        var twentyWeeksBeforeTo = to.DateTime.AddDays(-140);
        var weekSplitterFrom = from.Earlier(twentyWeeksBeforeTo)
            ? twentyWeeksBeforeTo
            : from.DateTime;

        var weekSplitter = new WeekSplitter(weekSplitterFrom, to.DateTime);

        var localSalaries = new List<UserSalarySimpleDto>();
        var remoteSalaries = new List<UserSalarySimpleDto>();

        foreach (var salary in salaries)
        {
            if (salary.Company is CompanyType.Local)
            {
                localSalaries.Add(salary);
            }
            else if (salary.Company is CompanyType.Foreign)
            {
                remoteSalaries.Add(salary);
            }
        }

        SalariesCountWeekByWeekChart = new SalariesCountWeekByWeekChart(
            localSalaries,
            remoteSalaries,
            weekSplitter,
            addGradeChartData);
    }

    public SalariesCountWeekByWeekChart SalariesCountWeekByWeekChart { get; private set; }

    public bool ShouldAddOwnSalary { get; private set; }

    public bool HasAuthentication { get; private set; }

    public DateTimeOffset From { get; private set; }

    public DateTimeOffset To { get; private set; }

    public DateTimeOffset ChartFrom { get; private set; }

    public DateTimeOffset ChartTo { get; private set; }

    public static GetSalariesHistoricalChartResponse NoSalaryOrAuthorization(
        bool hasAuthentication,
        bool shouldAddOwnSalary,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        return new GetSalariesHistoricalChartResponse
        {
            HasAuthentication = hasAuthentication,
            ShouldAddOwnSalary = shouldAddOwnSalary,
            From = from,
            To = to,
            ChartFrom = from,
            ChartTo = to,
        };
    }
}